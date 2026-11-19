using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using TikThat.Models;

namespace TikThat.Services;

public class BackupService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    // Export full user profile and task snapshot across desktop and mobile
    public async Task<bool> ExportBackupAsync(UserProfile profile, List<TodoItem> allTasks)
    {
        try
        {
            var backupData = new BackupDataModel
            {
                AppVersion = AppInfo.VersionString,
                BackupDate = DateTime.Now,
                Profile = profile,
                Tasks = allTasks ?? []
            };

            var jsonString = JsonSerializer.Serialize(backupData, _jsonOptions);
            var fileName = $"TikThat_Backup_{DateTime.Now:yyyyMMdd_HHmm}.json";

#if WINDOWS
            // Write directly to Desktop on Windows to bypass unreliable Share contract behavior
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var targetFilePath = Path.Combine(desktopPath, fileName);
            await File.WriteAllTextAsync(targetFilePath, jsonString);

            if (Application.Current?.Windows.FirstOrDefault()?.Page != null)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "بکاپ ایجاد شد", 
                    $"فایل پشتیبان با موفقیت روی دسکتاپ ذخیره شد:\n{fileName}", 
                    "باشه");
            }
            return true;
#else
            // Write to app cache and invoke native OS share sheet on mobile platforms
            var targetFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(targetFilePath, jsonString);

            // Marshal share sheet invocation onto the UI thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "پشتیبان داده‌های تیک دت",
                    File = new ShareFile(targetFilePath)
                });
            });

            return true;
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Backup Export Exception]: {ex.Message}");
            return false;
        }
    }

    // Import and merge backup snapshot without overwriting existing records
    public async Task<(bool success, string message, int importedCount)> ImportBackupAsync(
        UserProfile currentProfile,
        List<TodoItem> currentTasks,
        Func<TodoItem, Task> insertOrUpdateTaskInDb,
        Func<UserProfile, Task> updateProfileInDb)
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/json", "text/plain", "text/json", "*/*" } },
                { DevicePlatform.WinUI, new[] { ".json" } },
                { DevicePlatform.iOS, new[] { "public.json" } },
                { DevicePlatform.MacCatalyst, new[] { "public.json" } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "انتخاب فایل پشتیبان TikThat",
                FileTypes = customFileType
            });

            if (result == null)
                return (false, "فایلی انتخاب نشد.", 0);

            // Validate extension explicitly when Android file picker falls back to wildcard MIME types
            if (!result.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "لطفاً فقط فایل با پسوند .json انتخاب کنید.", 0);
            }

            using var stream = await result.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var jsonString = await reader.ReadToEndAsync();

            var backupData = JsonSerializer.Deserialize<BackupDataModel>(jsonString, _jsonOptions);
            if (backupData == null || backupData.Profile == null)
            {
                return (false, "فایل پشتیبان نامعتبر است یا ساختار درستی ندارد.", 0);
            }

            // Merge profile metrics, streaks, and unlocked achievements
            MergeProfile(currentProfile, backupData.Profile);
            await updateProfileInDb(currentProfile);

            // Deduplicate and append imported tasks
            int importedCount = 0;
            var existingKeys = new HashSet<string>(currentTasks.Select(t => $"{t.Text?.Trim()}_{t.DueDate:yyyyMMdd}"));

            if (backupData.Tasks != null)
            {
                foreach (var task in backupData.Tasks)
                {
                    var taskKey = $"{task.Text?.Trim()}_{task.DueDate:yyyyMMdd}";

                    bool hasValidId = !string.IsNullOrWhiteSpace(task.Id);
                    bool alreadyExists = (hasValidId && currentTasks.Any(t => t.Id == task.Id)) || existingKeys.Contains(taskKey);

                    if (!alreadyExists)
                    {
                        task.Id = Guid.NewGuid().ToString();
                        await insertOrUpdateTaskInDb(task);
                        currentTasks.Add(task);
                        existingKeys.Add(taskKey);
                        importedCount++;
                    }
                }
            }

            return (true, $"بکاپ با موفقیت اعمال شد. {importedCount} تسک جدید اضافه و جوایز همگام شدند.", importedCount);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Backup Import Exception]: {ex.Message}");
            return (false, $"خطا در خواندن فایل پشتیبان: {ex.Message}", 0);
        }
    }

    // Merge profile stats, achievements, and custom reminder schedules
    private void MergeProfile(UserProfile target, UserProfile imported)
    {
        target.UnlockedAchievements ??= [];
        target.UnlockedAchievementIds ??= [];
        target.DiscoveredSecretAchievementIds ??= [];

        imported.UnlockedAchievements ??= [];
        imported.UnlockedAchievementIds ??= [];
        imported.DiscoveredSecretAchievementIds ??= [];

        // Merge unlocked achievement display names
        foreach (var ach in imported.UnlockedAchievements)
        {
            if (!target.UnlockedAchievements.Contains(ach))
                target.UnlockedAchievements.Add(ach);
        }

        // Merge unlocked achievement IDs
        foreach (var achId in imported.UnlockedAchievementIds)
        {
            if (!target.UnlockedAchievementIds.Contains(achId))
                target.UnlockedAchievementIds.Add(achId);
        }

        // Merge discovered secret achievement IDs
        foreach (var secId in imported.DiscoveredSecretAchievementIds)
        {
            if (!target.DiscoveredSecretAchievementIds.Contains(secId))
                target.DiscoveredSecretAchievementIds.Add(secId);
        }

        // Retain highest progression and streak values across profiles
        target.Points = Math.Max(target.Points, imported.Points);
        target.Level = Math.Max(target.Level, imported.Level);
        target.CurrentStreak = Math.Max(target.CurrentStreak, imported.CurrentStreak);
        target.HighestStreak = Math.Max(target.HighestStreak, imported.HighestStreak);
        target.TotalTasksCompleted = Math.Max(target.TotalTasksCompleted, imported.TotalTasksCompleted);

        // Deduplicate and merge custom reminder rules by title
        if (imported.CustomReminders != null && imported.CustomReminders.Count > 0)
        {
            target.CustomReminders ??= [];
            var existingTitles = new HashSet<string>(target.CustomReminders.Select(r => r.Title?.Trim() ?? string.Empty));

            foreach (var rem in imported.CustomReminders)
            {
                if (!string.IsNullOrWhiteSpace(rem.Title) && !existingTitles.Contains(rem.Title.Trim()))
                {
                    target.CustomReminders.Add(rem);
                    existingTitles.Add(rem.Title.Trim());
                }
            }
        }

        if (imported.LastCompletionDate > target.LastCompletionDate)
        {
            target.LastCompletionDate = imported.LastCompletionDate;
        }
    }
}