using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using TikThat.Models;

namespace TikThat.Services;

public interface IUserProfileService
{
    Task<UserProfile> LoadProfileAsync();
    Task SaveProfileAsync(UserProfile profile);
}

public class UserProfileService : IUserProfileService
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);
    private UserProfile? _cachedProfile;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public UserProfileService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "userprofile.json");
    }

    public async Task<UserProfile> LoadProfileAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                _cachedProfile = new UserProfile();
                EnsureCollectionsInitialized(_cachedProfile);
                return _cachedProfile;
            }

            string json = await File.ReadAllTextAsync(_filePath);
            var loaded = JsonSerializer.Deserialize<UserProfile>(json, SerializerOptions) ?? new UserProfile();

            EnsureCollectionsInitialized(loaded);

            SyncAchievementCollections(loaded);

            _cachedProfile = loaded;
            return _cachedProfile;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserProfileService Error Loading]: {ex.Message}");
            return _cachedProfile ?? new UserProfile();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveProfileAsync(UserProfile profile)
    {
        if (profile == null) return;

        await _fileLock.WaitAsync();
        try
        {
            EnsureCollectionsInitialized(profile);
            SyncAchievementCollections(profile);

            string json = JsonSerializer.Serialize(profile, SerializerOptions);

            
// Atomic write via temp file to prevent corruption on unexpected shutdown
            string tempFile = _filePath + ".tmp";
            await File.WriteAllTextAsync(tempFile, json);

            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            File.Move(tempFile, _filePath);

            _cachedProfile = profile;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserProfileService Error Saving]: {ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private static void EnsureCollectionsInitialized(UserProfile profile)
    {
        profile.UnlockedAchievements ??= [];
        profile.UnlockedAchievementIds ??= [];
        profile.DiscoveredSecretAchievementIds ??= [];
        profile.CustomReminders ??= [];
    }

    private static void SyncAchievementCollections(UserProfile profile)
    {
        foreach (var id in profile.UnlockedAchievements)
        {
            if (!profile.UnlockedAchievementIds.Contains(id))
            {
                profile.UnlockedAchievementIds.Add(id);
            }
        }

        foreach (var id in profile.UnlockedAchievementIds)
        {
            if (!profile.UnlockedAchievements.Contains(id))
            {
                profile.UnlockedAchievements.Add(id);
            }
        }
    }
}