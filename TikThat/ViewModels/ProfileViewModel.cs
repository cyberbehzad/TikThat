using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using TikThat.Models;
using TikThat.Services;

namespace TikThat.ViewModels;

public partial class AchievementViewModel : ObservableObject
{
    public Achievement RawAchievement { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(DisplayDescription))]
    [NotifyPropertyChangedFor(nameof(DisplayGuide))]
    [NotifyPropertyChangedFor(nameof(IsSampleButtonVisible))]
    [NotifyPropertyChangedFor(nameof(CardBackground))]
    [NotifyPropertyChangedFor(nameof(CardBorder))]
    private bool _isUnlocked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(DisplayDescription))]
    [NotifyPropertyChangedFor(nameof(DisplayGuide))]
    [NotifyPropertyChangedFor(nameof(IsSampleButtonVisible))]
    [NotifyPropertyChangedFor(nameof(CardBackground))]
    [NotifyPropertyChangedFor(nameof(CardBorder))]
    private bool _isDiscovered;

    public double Opacity => (IsUnlocked || IsDiscovered || !RawAchievement.IsSecret) ? 1.0 : 0.45;

    public string StatusText => IsUnlocked
        ? "کسب شده ✓"
        : (IsDiscovered ? "کشف شد! در انتظار انجام" : (RawAchievement.IsSecret ? "قفل (کشف نشده)" : "در جریان"));

    public Color StatusColor => IsUnlocked
        ? Color.FromArgb("#4CAF50")
        : (IsDiscovered ? Color.FromArgb("#00E5FF") : Color.FromArgb("#8E8E93"));

    public Color CardBackground => IsUnlocked
        ? Color.FromArgb("#1E1E26")
        : (IsDiscovered ? Color.FromArgb("#171722") : Color.FromArgb("#131318"));

    public Color CardBorder => IsUnlocked
        ? Color.FromArgb("#8A4FFF")
        : (IsDiscovered ? Color.FromArgb("#00E5FF") : Color.FromArgb("#252530"));

    public string DisplayName => (!RawAchievement.IsSecret || IsUnlocked || IsDiscovered)
        ? RawAchievement.Name
        : "ماموریت مخفی 🔒";

    public string DisplayDescription => (!RawAchievement.IsSecret || IsUnlocked || IsDiscovered)
        ? RawAchievement.Description
        : "راز این تسک را با انجام کارهای روزمره کشف کن!";

    public string DisplayGuide => (!RawAchievement.IsSecret || IsUnlocked || IsDiscovered)
        ? RawAchievement.Guide
        : "برای کشف این مدال، کلمات روزمره را در فرم تسک‌ها تایپ کن.";

    public bool IsSampleButtonVisible => !IsUnlocked && (!RawAchievement.IsSecret || IsDiscovered) && !string.IsNullOrWhiteSpace(RawAchievement.SampleTaskText);

    public string DisplaySampleTask => RawAchievement.SampleTaskText;
    public string Icon => RawAchievement.Icon;

    public AchievementViewModel(Achievement achievement, bool isUnlocked, bool isDiscovered)
    {
        RawAchievement = achievement;
        _isUnlocked = isUnlocked;
        _isDiscovered = isDiscovered;
    }
}

public partial class ProfileViewModel : ObservableObject
{
    private readonly AchievementService _achievementService;
    private readonly IUserProfileService _profileService;
    private readonly PermissionService _permissionService;
    private readonly DatabaseService _databaseService;
    private readonly ITikThatNotificationService _notificationService;
    private readonly BackupService _backupService;

    [ObservableProperty]
    private UserProfile _userProfile = new();

    [ObservableProperty]
    private CustomReminder? _editingReminder;

    [ObservableProperty]
    private bool _isDayPickerModalVisible;

    public ObservableCollection<AchievementViewModel> Achievements { get; } = [];

    public ProfileViewModel(
        AchievementService achievementService,
        IUserProfileService profileService,
        PermissionService permissionService,
        DatabaseService databaseService,
        ITikThatNotificationService notificationService)
    {
        _achievementService = achievementService;
        _profileService = profileService;
        _permissionService = permissionService;
        _databaseService = databaseService;
        _notificationService = notificationService;
        _backupService = new BackupService();

        Task.Run(async () =>
        {
            var loadedProfile = await _profileService.LoadProfileAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UserProfile = loadedProfile;
                InitializeProfileData();
            });
        });
    }

    [RelayCommand]
    private async Task Appearing()
    {
        UserProfile = await _profileService.LoadProfileAsync();
        InitializeProfileData();
    }

    public void InitializeProfileData()
    {
        if (string.IsNullOrEmpty(UserProfile.ProfileImagePath))
        {
            UserProfile.ProfileImagePath = "user_avatar.png";
        }

        UserProfile.CustomReminders ??= [];

        var allAchievements = _achievementService.GetAllAchievements();

        var unlockedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (UserProfile.UnlockedAchievements != null)
        {
            foreach (var id in UserProfile.UnlockedAchievements)
                unlockedIds.Add(id);
        }

        if (UserProfile.UnlockedAchievementIds != null)
        {
            foreach (var id in UserProfile.UnlockedAchievementIds)
                unlockedIds.Add(id);
        }

        var discoveredIds = new HashSet<string>(UserProfile.DiscoveredSecretAchievementIds ?? [], StringComparer.OrdinalIgnoreCase);

        Achievements.Clear();
        foreach (var ach in allAchievements)
        {
            bool isUnlocked = unlockedIds.Contains(ach.Id);
            bool isDiscovered = discoveredIds.Contains(ach.Id);
            Achievements.Add(new AchievementViewModel(ach, isUnlocked, isDiscovered));
        }
    }

    // ==========================================
    
// --- Backup & Restore Commands ---
    // ==========================================

    [RelayCommand]
    private async Task CreateBackup()
    {
        try
        {
            var allTasks = await _databaseService.GetAllTasksAsync();
            bool success = await _backupService.ExportBackupAsync(UserProfile, allTasks);

            if (!success && Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطا", "ایجاد فایل پشتیبان با خطا مواجه شد.", "باشه");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CreateBackup Error]: {ex.Message}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطا", $"خطا در ایجاد پشتیبان: {ex.Message}", "باشه");
            }
        }
    }

    [RelayCommand]
    private async Task RestoreBackup()
    {
        if (Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "بازیابی اطلاعات",
            "تسک‌ها، مدال‌ها، یادآورها و امتیازهای فایل پشتیبان به برنامه افزوده خواهند شد و کارهای فعلی شما حفظ می‌شوند. آیا ادامه می‌دهید؟",
            "تایید",
            "انصراف");

        if (!confirm) return;

        try
        {
            var allTasks = await _databaseService.GetAllTasksAsync();

            var (success, message, importedCount) = await _backupService.ImportBackupAsync(
                UserProfile,
                allTasks.ToList(),
                async (task) => await _databaseService.SaveTaskAsync(task),
                async (profile) =>
                {
                    await _profileService.SaveProfileAsync(profile);
                    UserProfile = profile;
                });

            await Shell.Current.DisplayAlert(success ? "عملیات موفق" : "اطلاعیه", message, "باشه");

            if (success)
            {
                InitializeProfileData();
                OnPropertyChanged(nameof(UserProfile));

                var refreshedTasks = await _databaseService.GetAllTasksAsync();
                var todayTasks = refreshedTasks.Where(t => t.DueDate.Date == DateTime.Today).ToList();
                await _notificationService.RescheduleDailyRemindersAsync(UserProfile, todayTasks);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RestoreBackup Error]: {ex.Message}");
            await Shell.Current.DisplayAlert("خطا", $"خطا در بازیابی فایل: {ex.Message}", "باشه");
        }
    }

    [RelayCommand]
    private async Task SaveReminderSettings()
    {
        try
        {
            await _profileService.SaveProfileAsync(UserProfile);
            var tasks = await _databaseService.GetAllTasksAsync();
            var todayTasks = tasks.Where(t => t.DueDate.Date == DateTime.Today).ToList();

            await _notificationService.RescheduleDailyRemindersAsync(UserProfile, todayTasks);

            if (Shell.Current != null)
            {
                string statusMsg = UserProfile.IsDailyReminderEnabled
                    ? "زمان‌بندی یادآورهای روزانه با موفقیت ذخیره و فعال شد."
                    : "یادآورهای روزانه با موفقیت غیرفعال شدند.";

                await Shell.Current.DisplayAlert("تنظیمات یادآور", statusMsg, "باشه");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SaveReminderSettings Error]: {ex.Message}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطا", "مشکلی در ذخیره تنظیمات رخ داد.", "باشه");
            }
        }
    }

    [RelayCommand]
    private async Task AddCustomReminder()
    {
        UserProfile.CustomReminders ??= [];

        var reminder = new CustomReminder
        {
            Title = $"یادآور ویژه {UserProfile.CustomReminders.Count + 1}",
            WeeklySchedules =
            [
                new(DayOfWeek.Saturday, "شنبه", true, new TimeSpan(13, 0, 0)),
                new(DayOfWeek.Sunday, "یکشنبه", false, new TimeSpan(19, 0, 0)),
                new(DayOfWeek.Monday, "دوشنبه", false, new TimeSpan(19, 0, 0)),
                new(DayOfWeek.Tuesday, "سه‌شنبه", true, new TimeSpan(15, 0, 0)),
                new(DayOfWeek.Wednesday, "چهارشنبه", false, new TimeSpan(19, 0, 0)),
                new(DayOfWeek.Thursday, "پنجشنبه", false, new TimeSpan(19, 0, 0)),
                new(DayOfWeek.Friday, "جمعه", false, new TimeSpan(19, 0, 0))
            ]
        };

        UserProfile.CustomReminders.Add(reminder);
        await SaveReminderSettings();
    }

    [RelayCommand]
    private async Task RemoveCustomReminder(CustomReminder? reminder)
    {
        if (reminder == null) return;
        UserProfile.CustomReminders?.Remove(reminder);
        await SaveReminderSettings();
    }

    [RelayCommand]
    private void OpenDayPicker(CustomReminder? reminder)
    {
        if (reminder == null) return;
        EditingReminder = reminder;
        IsDayPickerModalVisible = true;
    }

    [RelayCommand]
    private async Task CloseDayPicker()
    {
        IsDayPickerModalVisible = false;
        EditingReminder?.NotifyDaysChanged();
        EditingReminder = null;
        await SaveReminderSettings();
    }

    [RelayCommand]
    private async Task CreateSampleTask(string? taskText)
    {
        if (string.IsNullOrWhiteSpace(taskText)) return;

        var navParams = new Dictionary<string, object>
        {
            { "NewTaskPreset", taskText }
        };

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..", navParams);
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    private async Task PickProfileImage()
    {
        bool hasPermission = await _permissionService.CheckAndRequestPhotoPermissionAsync();
        if (!hasPermission) return;

        try
        {
            var result = await FilePicker.Default.PickAsync(PickOptions.Images);
            if (result != null)
            {
                UserProfile.ProfileImagePath = result.FullPath;
                await _profileService.SaveProfileAsync(UserProfile);
                OnPropertyChanged(nameof(UserProfile));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PickProfileImage Error]: {ex.Message}");
        }
    }
}