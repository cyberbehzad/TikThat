using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Shiny;
using Shiny.Locations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TikThat.Messages;
using TikThat.Models;
using TikThat.Services;
using TikThat.Views;

namespace TikThat.ViewModels;

public partial class MainViewModel : ObservableObject, IQueryAttributable
{
    private const string WinSecurityPinKey = "TikThat_Windows_Security_PIN";

    private readonly IUserProfileService _profileService;
    private readonly AchievementService _achievementService;
    private readonly PermissionService _permissionService;
    private readonly DatabaseService _databaseService;
    private readonly IMessenger _messenger;
    private readonly IGeofenceManager _geofenceManager;
    private readonly ITikThatNotificationService _notificationService;
    private readonly BackupService _backupService;
    private readonly ObservableCollection<TodoItem> _allTasks = [];

    public event Action<string>? PlayAnimationRequested;

    [ObservableProperty]
    public partial UserProfile UserProfile { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<TodoItem> FilteredTasks { get; set; } = [];

    [ObservableProperty]
    public partial string NewItemText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedDateText { get; set; } = "امروز";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddTaskForSelectedDate))]
    public partial DateTime CurrentDate { get; set; } = DateTime.Today;

    public bool CanAddTaskForSelectedDate => CurrentDate.Date >= DateTime.Today;

    [ObservableProperty]
    public partial bool IsAddFormVisible { get; set; }

    [ObservableProperty]
    public partial bool AreAllTasksDone { get; set; }

    [ObservableProperty]
    public partial string ThemeIcon { get; set; } = "moon_icon.svg";

    [ObservableProperty]
    public partial bool IsDarkMode { get; set; } = true;

    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    [ObservableProperty]
    public partial string ProgressText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LocationButtonText { get; set; } = "افزودن مکان";

    [ObservableProperty]
    public partial bool HasTimeReminderForNewTask { get; set; }

    [ObservableProperty]
    public partial TimeSpan SelectedTaskTime { get; set; } = DateTime.Now.TimeOfDay.Add(TimeSpan.FromHours(1));

    [ObservableProperty]
    public partial string TimeButtonText { get; set; } = "⏰ تنظیم ساعت یادآوری";

    private double _tempLatitude;
    private double _tempLongitude;
    private bool _locationSetForNewTask;

    public MainViewModel(
        IUserProfileService profileService,
        AchievementService achievementService,
        PermissionService permissionService,
        DatabaseService databaseService,
        IMessenger messenger,
        IGeofenceManager geofenceManager,
        ITikThatNotificationService notificationService)
    {
        _profileService = profileService;
        _achievementService = achievementService;
        _permissionService = permissionService;
        _databaseService = databaseService;
        _messenger = messenger;
        _geofenceManager = geofenceManager;
        _notificationService = notificationService;
        _backupService = new BackupService();

        _messenger.Register<CreateSampleTaskMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await _databaseService.SaveTaskAsync(m.Value);
                _allTasks.Add(m.Value);
                FilterTasksForDate(m.Value.DueDate);
                await _profileService.SaveProfileAsync(UserProfile);
                await RefreshDailyRemindersAsync();
            });
        });

        _messenger.Register<LocationPickedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var msgType = m.GetType();
                    var latProp = msgType.GetProperty("Latitude");
                    var lonProp = msgType.GetProperty("Longitude");

                    if (latProp != null && lonProp != null)
                    {
                        var latVal = latProp.GetValue(m);
                        var lonVal = lonProp.GetValue(m);
                        if (latVal is double lat && lonVal is double lon)
                        {
                            _tempLatitude = lat;
                            _tempLongitude = lon;
                            _locationSetForNewTask = true;
                            LocationButtonText = "✅ مکان ثبت شد";
                            return;
                        }
                    }

                    var valProp = msgType.GetProperty("Value");
                    string? rawVal = valProp?.GetValue(m)?.ToString();

                    if (!string.IsNullOrWhiteSpace(rawVal))
                    {
                        var locationData = JsonSerializer.Deserialize<Dictionary<string, double>>(rawVal);
                        if (locationData != null && locationData.TryGetValue("latitude", out var lat) && locationData.TryGetValue("longitude", out var lon))
                        {
                            _tempLatitude = lat;
                            _tempLongitude = lon;
                            _locationSetForNewTask = true;
                            LocationButtonText = "✅ مکان ثبت شد";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LocationPicked] Parsing payload failed: {ex.Message}");
                }
            });
        });

        Task.Run(async () =>
        {
            UserProfile = await _profileService.LoadProfileAsync();
            var dbTasks = await _databaseService.GetAllTasksAsync();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _allTasks.Clear();
                foreach (var task in dbTasks)
                {
                    _allTasks.Add(task);
                }

                if (!_allTasks.Any())
                {
                    await LoadInitialTasksAsync();
                }

                FilterTasksForDate(DateTime.Today);
                await RefreshDailyRemindersAsync();
            });
        });
    }

    private async Task LoadInitialTasksAsync()
    {
        if (_allTasks.Any()) return;

        var defaultTasks = new List<TodoItem>
        {
            new() { Text = "برنامه ریزی کارهای روزانه با تیک ذت", DueDate = DateTime.Today, IsSwipeHintVisible = true },
            new() { Text = "یادداشت های محرمانه و شخصی", DueDate = DateTime.Today, IsLocked = true },
            new() { Text = "تمرکز عمیق روی تسک مهم با نینجا مود", DueDate = DateTime.Today }
        };

        foreach (var task in defaultTasks)
        {
            await _databaseService.SaveTaskAsync(task);
            _allTasks.Add(task);
        }
    }

    private void FilterTasksForDate(DateTime date)
    {
        CurrentDate = date;
        var tasksForDate = _allTasks.Where(t => t.DueDate.Date == date.Date).ToList();

        FilteredTasks.Clear();
        foreach (var item in tasksForDate)
        {
            FilteredTasks.Add(item);
        }

        var shamsiDate = new CustomShamsiDate(date);
        SelectedDateText = date.Date == DateTime.Today.Date ? "امروز" : $"{shamsiDate.Day} {shamsiDate.MonthName}";
        UpdateProgress();
        UpdateSwipeHint();
    }

    private void UpdateProgress()
    {
        var completedCount = FilteredTasks.Count(t => t.IsCompleted);
        var totalCount = FilteredTasks.Count;
        if (totalCount == 0)
        {
            ProgressValue = 0;
            ProgressText = "برای این روز تسکی نداری!";
        }
        else
        {
            ProgressValue = (double)completedCount / totalCount;
            ProgressText = $"{completedCount} از {totalCount} تسک انجام شد";
        }
        AreAllTasksDone = totalCount > 0 && completedCount == totalCount;
    }

    private void UpdateSwipeHint()
    {
        for (int i = 0; i < FilteredTasks.Count; i++)
        {
            FilteredTasks[i].IsSwipeHintVisible = (i == 0 && !FilteredTasks[i].IsCompleted);
        }
    }

    private async Task RefreshDailyRemindersAsync()
    {
        try
        {
            await _notificationService.RescheduleDailyRemindersAsync(UserProfile, _allTasks);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Reminders] Rescheduling worker faulted: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemText)) return;

        if (!CanAddTaskForSelectedDate)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("خطا", "امکان افزودن تسک برای روزهای گذشته وجود ندارد.", "باشه");
            return;
        }

        var taskText = NewItemText.Trim();
        var isLocationTask = _locationSetForNewTask;
        var lat = _tempLatitude;
        var lon = _tempLongitude;
        var hasTimeReminder = HasTimeReminderForNewTask;
        var reminderTime = SelectedTaskTime;

        NewItemText = string.Empty;
        _locationSetForNewTask = false;
        LocationButtonText = "افزودن مکان";
        HasTimeReminderForNewTask = false;
        TimeButtonText = "⏰ تنظیم ساعت یادآوری";
        IsAddFormVisible = false;

        var newItem = new TodoItem
        {
            Text = taskText,
            DueDate = CurrentDate,
            HasLocationReminder = isLocationTask,
            Latitude = isLocationTask ? lat : 0,
            Longitude = isLocationTask ? lon : 0,
            LocationName = isLocationTask ? "یادآور مکانی فعال است" : string.Empty,
            HasTimeReminder = hasTimeReminder,
            ReminderTime = hasTimeReminder ? reminderTime : null
        };

        try
        {
            await _databaseService.SaveTaskAsync(newItem);
            _allTasks.Add(newItem);
            FilterTasksForDate(CurrentDate);

            var discoveryResult = _achievementService.CheckDiscoveryOnTaskCreated(taskText, UserProfile);
            if (discoveryResult.NewlyDiscoveredAchievements.Count > 0)
            {
                await _profileService.SaveProfileAsync(UserProfile);
                var firstAch = discoveryResult.NewlyDiscoveredAchievements[0];

                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert(
                        "🔍 ایول! یک تسک مخفی پیدا کردی!",
                        $"شما راز دستاورد «{firstAch.Name}» را کشف کردید!\n\nوارد پروفایل شوید تا دکمه تسک نمونه برای شما فعال شود. با انجام و تیک زدن کامل آن، مدال و ۵۰ امتیاز را دریافت خواهید کرد.",
                        "متوجه شدم");
                }
            }
            else
            {
                await _profileService.SaveProfileAsync(UserProfile);
            }

            if (newItem.HasTimeReminder)
            {
                await _notificationService.ScheduleTaskReminderAsync(newItem);
            }

            await RefreshDailyRemindersAsync();

#if !WINDOWS
            if (isLocationTask)
            {
                await _permissionService.EnsureGpsKeepAliveAlertAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        bool hasBgPermission = await _permissionService.CheckAndRequestBackgroundLocationAsync();
                        if (hasBgPermission)
                        {
                            var access = await _geofenceManager.RequestAccess();
                            if (access == AccessState.Available)
                            {
                                await _geofenceManager.StartMonitoring(new GeofenceRegion(
                                    newItem.Id.ToString(),
                                    new Position(newItem.Latitude, newItem.Longitude),
                                    Distance.FromMeters(200)
                                ));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Geofence] Background region watch setup failed: {ex.Message}");
                    }
                });
            }
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AddItem] Persistence transaction failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SetLocationReminder()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(MapPickerPage));
        }
    }

    [RelayCommand]
    private async Task ToggleCompleteItem(TodoItem? item)
    {
        if (item is null || item.IsLocked) return;

        bool wasCompleted = item.IsCompleted;
        item.IsCompleted = !item.IsCompleted;

        await _databaseService.SaveTaskAsync(item);

        if (item.IsCompleted && !wasCompleted)
        {
            await _notificationService.CancelTaskReminderAsync(item);

            UserProfile.Points += 10;
            UserProfile.TotalTasksCompleted++;
            UpdateStreak();

            PlayAnimationRequested?.Invoke("checkmark_animation.json");

            var unlockResult = _achievementService.ProcessTaskCompletion(item, UserProfile, UserProfile.TotalTasksCompleted);
            if (unlockResult.NewlyUnlockedAchievements.Count > 0)
            {
                await _profileService.SaveProfileAsync(UserProfile);

                foreach (var ach in unlockResult.NewlyUnlockedAchievements)
                {
                    if (Shell.Current != null)
                    {
                        string title = ach.IsSecret ? "🎉 دستاورد مخفی آنلاک شد!" : "🏆 مدال جدید کسب شد!";
                        string message = $"تبریک! شما موفق به کسب «{ach.Name}» شدید.\n\nتوضیح: {ach.Description}\n+۵۰ امتیاز به پروفایل اضافه شد!";
                        await Shell.Current.DisplayAlert(title, message, "عالیه!");
                    }
                }
            }
        }
        else if (!item.IsCompleted && wasCompleted)
        {
            if (item.HasTimeReminder)
            {
                await _notificationService.ScheduleTaskReminderAsync(item);
            }

            UserProfile.Points = Math.Max(0, UserProfile.Points - 10);
            UserProfile.TotalTasksCompleted = Math.Max(0, UserProfile.TotalTasksCompleted - 1);
        }

        await _profileService.SaveProfileAsync(UserProfile);
        UpdateProgress();
        UpdateSwipeHint();
        await RefreshDailyRemindersAsync();
    }

    private void UpdateStreak()
    {
        var today = DateTime.Today;
        if (UserProfile.LastCompletionDate.Date == today) return;

        if (UserProfile.LastCompletionDate.Date == today.AddDays(-1))
            UserProfile.CurrentStreak++;
        else
            UserProfile.CurrentStreak = 1;

        if (UserProfile.CurrentStreak > UserProfile.HighestStreak)
            UserProfile.HighestStreak = UserProfile.CurrentStreak;

        UserProfile.LastCompletionDate = today;
    }

    [RelayCommand]
    private async Task DeleteItem(TodoItem? item)
    {
        if (item is null) return;

        await _notificationService.CancelTaskReminderAsync(item);

#if !WINDOWS
        if (item.HasLocationReminder)
        {
            try
            {
                await _geofenceManager.StopMonitoring(item.Id.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Geofence] Monitoring teardown failed: {ex.Message}");
            }
        }
#endif

        await _databaseService.DeleteTaskAsync(item);
        _allTasks.Remove(item);

        PlayAnimationRequested?.Invoke("delete_animation.json");

        await _profileService.SaveProfileAsync(UserProfile);
        await Task.Delay(500);
        FilterTasksForDate(CurrentDate);

        await RefreshDailyRemindersAsync();
    }

    [RelayCommand]
    private async Task ToggleLock(TodoItem? item)
    {
        if (item is null) return;

        if (!item.IsLocked)
        {
            item.IsLocked = true;
            await _databaseService.SaveTaskAsync(item);
            PlayAnimationRequested?.Invoke("lock_animation.json");
            return;
        }

        bool isAuthenticated = false;

#if WINDOWS
        // Windows fallback: self-contained app-level PIN auth
        string savedPin = Preferences.Default.Get(WinSecurityPinKey, string.Empty);

        if (string.IsNullOrWhiteSpace(savedPin))
        {
            if (Shell.Current != null)
            {
                string newPin = await Shell.Current.DisplayPromptAsync(
                    "تعریف رمز تسک‌ها",
                    "برای امنیت تسک‌های قفل‌شده در ویندوز، یک رمز عبور یا پین تعیین کنید:",
                    "ذخیره رمز",
                    "انصراف",
                    placeholder: "مثال: 1234",
                    maxLength: 20,
                    keyboard: Keyboard.Text);

                if (!string.IsNullOrWhiteSpace(newPin))
                {
                    Preferences.Default.Set(WinSecurityPinKey, newPin.Trim());
                    await Shell.Current.DisplayAlert("تایید", "رمز شما ذخیره شد. از این پس برای باز کردن تسک‌ها در ویندوز از این رمز استفاده کنید.", "باشه");
                    isAuthenticated = true;
                }
                else
                {
                    return;
                }
            }
        }
        else
        {
            if (Shell.Current != null)
            {
                string inputPin = await Shell.Current.DisplayPromptAsync(
                    "احراز هویت",
                    "رمز عبور تسک‌های محرمانه را وارد کنید:",
                    "تایید",
                    "انصراف",
                    placeholder: "رمز عبور",
                    maxLength: 20,
                    keyboard: Keyboard.Text);

                if (!string.IsNullOrWhiteSpace(inputPin))
                {
                    if (inputPin.Trim() == savedPin)
                    {
                        isAuthenticated = true;
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("خطا", "رمز واردشده نادرست است!", "تلاش مجدد");
                        return;
                    }
                }
            }
        }
#else
        // Mobile runtime: delegate to native biometrics/keystore fallback
        try
        {
            var isAvailable = await CrossFingerprint.Current.IsAvailableAsync();
            if (isAvailable)
            {
                var request = new AuthenticationRequestConfiguration("احراز هویت", "برای باز کردن قفل، اثر انگشت یا رمز گوشی را تایید کنید");
                var result = await CrossFingerprint.Current.AuthenticateAsync(request);
                isAuthenticated = result.Authenticated;
            }
            else if (Shell.Current != null)
            {
                isAuthenticated = await Shell.Current.DisplayAlert("احراز هویت", "آیا هویت شما برای باز کردن تسک تایید می‌شود؟", "بله", "خیر");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Security] Biometric pipeline faulted: {ex.Message}");
            isAuthenticated = false;
        }
#endif

        if (isAuthenticated)
        {
            item.IsLocked = false;
            await _databaseService.SaveTaskAsync(item);
            PlayAnimationRequested?.Invoke("unlock_animation.json");
        }
        else if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("خطا", "احراز هویت انجام نشد؛ تسک قفل باقی ماند.", "باشه");
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        }
        ThemeIcon = IsDarkMode ? "moon_icon.svg" : "sun_icon.svg";
    }

    [RelayCommand]
    private async Task ShowAddForm()
    {
        if (!CanAddTaskForSelectedDate)
        {
            if (Shell.Current != null)
                await Shell.Current.DisplayAlert("خطا", "امکان افزودن تسک برای روزهای گذشته وجود ندارد.", "باشه");
            return;
        }

        IsAddFormVisible = true;
    }

    [RelayCommand]
    private void HideAddForm()
    {
        IsAddFormVisible = false;
        _locationSetForNewTask = false;
        LocationButtonText = "افزودن مکان";
        HasTimeReminderForNewTask = false;
    }

    [RelayCommand]
    private async Task GoToCalendar()
    {
        if (Shell.Current == null) return;

        var taskDates = _allTasks.Select(t => t.DueDate.Date).Distinct().ToList();
        var navigationParameters = new Dictionary<string, object>
        {
            { "CurrentDate", CurrentDate },
            { "TaskDates", taskDates }
        };
        await Shell.Current.GoToAsync(nameof(CalendarPage), navigationParameters);
    }

    [RelayCommand]
    private async Task GoToProfile()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync(nameof(ProfilePage));
        }
    }

    [RelayCommand]
    private async Task StartNinjaMode(TodoItem? item)
    {
        if (item is null || Shell.Current == null) return;

        var navigationParameters = new Dictionary<string, object>
        {
            { "TaskToFocus", item }
        };
        await Shell.Current.GoToAsync(nameof(NinjaModeSetupPage), navigationParameters);
    }

    [RelayCommand]
    private async Task CreateBackup()
    {
        try
        {
            var allDbTasks = await _databaseService.GetAllTasksAsync();
            bool success = await _backupService.ExportBackupAsync(UserProfile, allDbTasks);

            if (!success && Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطا", "ایجاد فایل پشتیبان با خطا مواجه شد.", "باشه");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Backup] Export archive pipeline failed: {ex.Message}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("خطا", $"خطا در پشتیبان‌گیری: {ex.Message}", "باشه");
            }
        }
    }

    [RelayCommand]
    private async Task RestoreBackup()
    {
        if (Shell.Current == null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "بازیابی اطلاعات",
            "تسک‌ها، مدال‌ها و امتیازهای فایل پشتیبان به برنامه اضافه خواهند شد و کارهای فعلی شما محفوظ می‌مانند. مایل به ادامه هستید؟",
            "تایید",
            "انصراف");

        if (!confirm) return;

        try
        {
            var allDbTasks = await _databaseService.GetAllTasksAsync();

            var (success, message, importedCount) = await _backupService.ImportBackupAsync(
                UserProfile,
                allDbTasks.ToList(),
                async (task) =>
                {
                    await _databaseService.SaveTaskAsync(task);
                },
                async (profile) =>
                {
                    await _profileService.SaveProfileAsync(profile);
                });

            await Shell.Current.DisplayAlert(success ? "عملیات موفق" : "اطلاعیه", message, "باشه");

            if (success)
            {
                var refreshedDbTasks = await _databaseService.GetAllTasksAsync();
                _allTasks.Clear();
                foreach (var task in refreshedDbTasks)
                {
                    _allTasks.Add(task);
                }

                FilterTasksForDate(CurrentDate);
                OnPropertyChanged(nameof(UserProfile));
                await RefreshDailyRemindersAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Backup] Import payload parsing aborted: {ex.Message}");
            await Shell.Current.DisplayAlert("خطا", $"خطا در بازیابی اطلاعات: {ex.Message}", "باشه");
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("SelectedDate", out var selectedDateObj) && selectedDateObj is DateTime date)
        {
            FilterTasksForDate(date);
        }

        if (query.TryGetValue("SelectedLat", out var latObj) && query.TryGetValue("SelectedLng", out var lngObj))
        {
            if (latObj is double lat && lngObj is double lng)
            {
                _tempLatitude = lat;
                _tempLongitude = lng;
                _locationSetForNewTask = true;
                LocationButtonText = "✅ مکان ثبت شد";
            }
        }

        if (query.TryGetValue("NewTaskPreset", out var presetObj) && presetObj is string taskTitle && !string.IsNullOrWhiteSpace(taskTitle))
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var sampleItem = new TodoItem
                {
                    Text = taskTitle.Trim(),
                    DueDate = CurrentDate
                };

                await _databaseService.SaveTaskAsync(sampleItem);
                _allTasks.Insert(0, sampleItem);
                FilterTasksForDate(CurrentDate);
                await _profileService.SaveProfileAsync(UserProfile);
                await RefreshDailyRemindersAsync();
            });
        }
    }

    public void OnAppearing()
    {
        FilterTasksForDate(CurrentDate);
    }
}