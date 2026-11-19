using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TikThat.Models;

#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
#endif

namespace TikThat.Services;

/// <summary>
/// Cross-platform notification orchestration service handling immediate dispatches,
/// persistent background scheduling, daily operational cadences, and granular weekly cycles.
/// </summary>
public class NotificationService : ITikThatNotificationService
{
    private const int IdMorning = 1001;
    private const int IdNoon = 1002;
    private const int IdEvening = 1003;
    private const int IdNight = 1004;

    public const string ChannelTaskReminders = "tikthat_task_reminders";
    public const string ChannelDailyReminders = "tikthat_daily_reminders";
    public const string ChannelGeofence = "tikthat_geofence_channel";

#if WINDOWS
    /// <summary>
    /// Represents an in-memory scheduled notification node with recurring interval metadata.
    /// </summary>
    private sealed class WindowsScheduledItem
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public DateTime NotifyTime { get; set; }
        public bool IsDaily { get; init; }
        public bool IsWeekly { get; init; }
    }

    private static Timer? _systemWatcherTimer;
    private static readonly object _syncLock = new();
    private static readonly List<WindowsScheduledItem> _activeReminders = new();
#endif

    public NotificationService()
    {
#if WINDOWS
        EnsureWatcherInitialized();
#endif
    }

    /// <summary>
    /// Verifies and requests runtime notification dispatch permissions across target platforms.
    /// </summary>
    public async Task<bool> RequestPermissionAsync()
    {
#if WINDOWS
        return await Task.FromResult(true);
#else
        try
        {
            bool isGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!isGranted)
            {
                isGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            if (!isGranted && Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("دسترسی لازم است", "برای ارسال یادآورهای زمان و مکان، دسترسی نوتیفیکیشن را فعال کنید.", "باشه");
                return false;
            }

            return isGranted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NotificationService Permission Fault]: {ex.Message}");
            return false;
        }
#endif
    }

    /// <summary>
    /// Dispatches an immediate high-priority toast notification to the active shell.
    /// </summary>
    public async Task ShowNotification(string title, string body, int id = 0)
    {
        int notifId = id == 0 ? Random.Shared.Next(1000, 9999) : id;

#if WINDOWS
        ExecuteWindowsNotificationImmediately(title, body);
        await Task.CompletedTask;
#else
        bool hasPermission = await RequestPermissionAsync();
        if (!hasPermission) return;

        var request = new NotificationRequest
        {
            NotificationId = notifId,
            Title = title,
            Description = body,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(1)
            },
            Android = new AndroidOptions
            {
                ChannelId = ChannelTaskReminders,
                Priority = AndroidPriority.High
            }
        };

        await LocalNotificationCenter.Current.Show(request);
#endif
    }

    /// <summary>
    /// Enqueues a specific task-bound reminder based on due date and allocated target time.
    /// </summary>
    public async Task ScheduleTaskReminderAsync(TodoItem task)
    {
        if (!task.HasTimeReminder || !task.ReminderTime.HasValue || task.IsCompleted)
            return;

        var targetDateTime = task.DueDate.Date + task.ReminderTime.Value;
        if (targetDateTime <= DateTime.Now)
            return;

        if (task.NotificationId == 0)
        {
            task.NotificationId = Random.Shared.Next(10000, 99999);
        }

        string title = "⏰ زمان انجام تسک رسید!";
        string description = $"یادآوری: «{task.Text}»";

#if WINDOWS
        ScheduleWindowsInMemory(task.NotificationId, title, description, targetDateTime, isDaily: false, isWeekly: false);
        await Task.CompletedTask;
#else
        bool hasPermission = await RequestPermissionAsync();
        if (!hasPermission) return;

        var request = new NotificationRequest
        {
            NotificationId = task.NotificationId,
            Title = title,
            Description = description,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = targetDateTime
            },
            Android = new AndroidOptions
            {
                ChannelId = ChannelTaskReminders,
                Priority = AndroidPriority.High
            }
        };

        await LocalNotificationCenter.Current.Show(request);
#endif
    }

    /// <summary>
    /// Purges a registered task reminder from the scheduler queue.
    /// </summary>
    public Task CancelTaskReminderAsync(TodoItem task)
    {
        if (task.NotificationId > 0)
        {
#if WINDOWS
            lock (_syncLock)
            {
                _activeReminders.RemoveAll(t => t.Id == task.NotificationId);
            }
#else
            LocalNotificationCenter.Current.Cancel(task.NotificationId);
#endif
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Re-evaluates and resets both daily operational checkpoints and custom recurring matrices.
    /// </summary>
    public async Task RescheduleDailyRemindersAsync(UserProfile profile, IEnumerable<TodoItem> allTasks)
    {
#if WINDOWS
        lock (_syncLock)
        {
            _activeReminders.RemoveAll(t => t.Id is IdMorning or IdNoon or IdEvening or IdNight);

            if (profile.CustomReminders != null)
            {
                foreach (var custom in profile.CustomReminders)
                {
                    _activeReminders.RemoveAll(t => t.Id == custom.Id);
                    for (int d = 0; d < 7; d++)
                    {
                        int subId = custom.Id * 10 + d;
                        _activeReminders.RemoveAll(t => t.Id == subId);
                    }
                }
            }
        }
#else
        LocalNotificationCenter.Current.Cancel(new[] { IdMorning, IdNoon, IdEvening, IdNight });

        if (profile.CustomReminders != null)
        {
            foreach (var custom in profile.CustomReminders)
            {
                LocalNotificationCenter.Current.Cancel(custom.Id);
                for (int d = 0; d < 7; d++)
                {
                    LocalNotificationCenter.Current.Cancel(custom.Id * 10 + d);
                }
            }
        }
#endif

        if (!profile.IsDailyReminderEnabled) return;

        bool hasPermission = await RequestPermissionAsync();
        if (!hasPermission) return;

        var taskList = allTasks?.ToList() ?? new List<TodoItem>();
        var todayPendingCount = taskList.Count(t => t.DueDate.Date == DateTime.Today && !t.IsCompleted);

        // Standard operational reminders trigger regardless of zero task count to maintain user engagement
        string morningDescription = todayPendingCount > 0
            ? $"شما {todayPendingCount} تسک باز برای امروز دارید."
            : "روز خود را پرانرژی آغاز کنید! کارهای امروز را مشخص کنید.";

        if (profile.IsMorningReminderEnabled)
            await ScheduleSingleDaily(IdMorning, profile.MorningReminderTime, "🌅 برنامه صبحگاهی تیک دت", morningDescription);

        if (profile.IsNoonReminderEnabled)
            await ScheduleSingleDaily(IdNoon, profile.NoonReminderTime, "☀️ پیگیری ظهر", "نیمه روز سپری شد! بررسی کنید چه کارهایی تکمیل شده است.");

        if (profile.IsEveningReminderEnabled)
            await ScheduleSingleDaily(IdEvening, profile.EveningReminderTime, "🌇 تمرکز عصرگاهی", "فرصت مناسبی برای تکمیل تسک‌های باقی‌مانده است.");

        if (profile.IsNightReminderEnabled)
            await ScheduleSingleDaily(IdNight, profile.NightReminderTime, "🌙 بررسی پایانی شب", $"کارهای امروز را نهایی کنید تا زنجیره {profile.CurrentStreak} روزه ثبت شود.");

        if (profile.CustomReminders != null)
        {
            foreach (var custom in profile.CustomReminders.Where(c => c.IsEnabled && c.WeeklySchedules != null))
            {
                await ScheduleCustomWeeklySchedulesAsync(custom, taskList);
            }
        }
    }

    /// <summary>
    /// Calculates the next occurrence timestamp for a recurring daily checkpoint and registers it.
    /// </summary>
    private async Task ScheduleSingleDaily(int id, TimeSpan time, string title, string description)
    {
        var targetTime = DateTime.Today.Add(time);
        if (targetTime <= DateTime.Now)
            targetTime = targetTime.AddDays(1);

#if WINDOWS
        ScheduleWindowsInMemory(id, title, description, targetTime, isDaily: true, isWeekly: false);
        await Task.CompletedTask;
#else
        var request = new NotificationRequest
        {
            NotificationId = id,
            Title = title,
            Description = description,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = targetTime,
                RepeatType = NotificationRepeat.Daily
            },
            Android = new AndroidOptions
            {
                ChannelId = ChannelDailyReminders,
                Priority = AndroidPriority.High
            }
        };

        await LocalNotificationCenter.Current.Show(request);
#endif
    }

    /// <summary>
    /// Processes weekly recurrence intervals based on user-selected day flags and designated time slots.
    /// </summary>
    private async Task ScheduleCustomWeeklySchedulesAsync(CustomReminder custom, IEnumerable<TodoItem> allTasks)
    {
        if (custom.WeeklySchedules == null) return;

        var activeDays = custom.WeeklySchedules.Where(s => s.IsSelected).ToList();
        if (activeDays.Count == 0) return;

        var now = DateTime.Now;
        var taskList = allTasks.ToList();

        foreach (var dayItem in activeDays)
        {
            DateTime nextOccurrence = DateTime.Today.Add(dayItem.ScheduledTime);
            int daysUntilTarget = ((int)dayItem.Day - (int)DateTime.Today.DayOfWeek + 7) % 7;
            nextOccurrence = nextOccurrence.AddDays(daysUntilTarget);

            if (nextOccurrence <= now)
                nextOccurrence = nextOccurrence.AddDays(7);

            var tasksForThatDayCount = taskList.Count(t => t.DueDate.Date == nextOccurrence.Date && !t.IsCompleted);
            string description = tasksForThatDayCount > 0
                ? $"یادآوری: شما {tasksForThatDayCount} تسک باز برای این روز دارید."
                : "زمان بررسی و مدیریت کارهای تیک دت فرا رسید.";

            int subNotificationId = custom.Id * 10 + (int)dayItem.Day;
            string title = $"⏰ {custom.Title} ({dayItem.DayTitle})";

#if WINDOWS
            ScheduleWindowsInMemory(subNotificationId, title, description, nextOccurrence, isDaily: false, isWeekly: true);
#else
            var request = new NotificationRequest
            {
                NotificationId = subNotificationId,
                Title = title,
                Description = description,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = nextOccurrence,
                    RepeatType = NotificationRepeat.Weekly
                },
                Android = new AndroidOptions
                {
                    ChannelId = ChannelDailyReminders,
                    Priority = AndroidPriority.High
                }
            };

            await LocalNotificationCenter.Current.Show(request);
#endif
        }
    }

#if WINDOWS
    /// <summary>
    /// Inserts or replaces a notification item in the thread-safe memory queue.
    /// </summary>
    private static void ScheduleWindowsInMemory(int id, string title, string body, DateTime deliveryTime, bool isDaily, bool isWeekly)
    {
        EnsureWatcherInitialized();

        lock (_syncLock)
        {
            _activeReminders.RemoveAll(t => t.Id == id);
            _activeReminders.Add(new WindowsScheduledItem
            {
                Id = id,
                Title = title,
                Body = body,
                NotifyTime = deliveryTime,
                IsDaily = isDaily,
                IsWeekly = isWeekly
            });
        }

        Debug.WriteLine($"[Watcher Queue] Enqueued ID {id} ('{title}') for {deliveryTime:yyyy-MM-dd HH:mm:ss} (Daily: {isDaily}, Weekly: {isWeekly})");
    }

    /// <summary>
    /// Backward-compatible bridge for legacy one-off scheduling requests.
    /// </summary>
    public static void ScheduleNativeWindowsToast(int id, string title, string body, DateTime deliveryTime)
    {
        ScheduleWindowsInMemory(id, title, body, deliveryTime, isDaily: false, isWeekly: false);
    }

    /// <summary>
    /// Ensures the high-resolution background polling timer is instantiated.
    /// </summary>
    private static void EnsureWatcherInitialized()
    {
        if (_systemWatcherTimer != null) return;
        _systemWatcherTimer = new Timer(OnWatcherTick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Evaluates due notification queues, triggers native toast payloads, and rolls recurrence timelines.
    /// </summary>
    private static void OnWatcherTick(object? state)
    {
        try
        {
            var now = DateTime.Now;
            List<WindowsScheduledItem> dueItems;

            lock (_syncLock)
            {
                dueItems = _activeReminders.Where(t => t.NotifyTime <= now).ToList();

                foreach (var item in dueItems)
                {
                    if (item.IsDaily)
                    {
                        item.NotifyTime = item.NotifyTime.AddDays(1);
                    }
                    else if (item.IsWeekly)
                    {
                        item.NotifyTime = item.NotifyTime.AddDays(7);
                    }
                    else
                    {
                        _activeReminders.Remove(item);
                    }
                }
            }

            foreach (var item in dueItems)
            {
                ExecuteWindowsNotificationImmediately(item.Title, item.Body);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Watcher Dispatch Fault]: {ex.Message}");
        }
    }

    /// <summary>
    /// Dispatches a native WinUI 3 / UWP Toast XML payload to the Windows Action Center.
    /// </summary>
    private static void ExecuteWindowsNotificationImmediately(string title, string description)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(description)
                .Show(toast =>
                {
                    toast.Priority = ToastNotificationPriority.High;
                });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Toast Dispatch Exception]: {ex.Message}");
        }
    }
#endif
}