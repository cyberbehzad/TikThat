using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TikThat.Models;

public partial class DayScheduleItem : ObservableObject
{
    public DayOfWeek Day { get; set; }

    [ObservableProperty]
    public partial string DayTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial TimeSpan ScheduledTime { get; set; } = new TimeSpan(19, 0, 0);

    public DayScheduleItem() { }

    public DayScheduleItem(DayOfWeek day, string dayTitle, bool isSelected, TimeSpan time)
    {
        Day = day;
        DayTitle = dayTitle;
        IsSelected = isSelected;
        ScheduledTime = time;
    }
}

public partial class CustomReminder : ObservableObject
{
    public int Id { get; set; } = Random.Shared.Next(2000, 8999);

    [ObservableProperty]
    public partial string Title { get; set; } = "یادآور روزهای انتخابی";

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    // Default fallback trigger time
    [ObservableProperty]
    public partial TimeSpan Time { get; set; } = new TimeSpan(19, 0, 0);

    // Per-day schedule configuration with independent trigger times
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveDaysSummary))]
    public partial ObservableCollection<DayScheduleItem> WeeklySchedules { get; set; } =
    [
        new(DayOfWeek.Saturday, "شنبه", true, new TimeSpan(13, 0, 0)),
        new(DayOfWeek.Sunday, "یکشنبه", false, new TimeSpan(19, 0, 0)),
        new(DayOfWeek.Monday, "دوشنبه", false, new TimeSpan(19, 0, 0)),
        new(DayOfWeek.Tuesday, "سه‌شنبه", true, new TimeSpan(15, 0, 0)),
        new(DayOfWeek.Wednesday, "چهارشنبه", false, new TimeSpan(19, 0, 0)),
        new(DayOfWeek.Thursday, "پنجشنبه", false, new TimeSpan(19, 0, 0)),
        new(DayOfWeek.Friday, "جمعه", false, new TimeSpan(19, 0, 0))
    ];

    // Projected active days collection for legacy consumer compatibility
    public ObservableCollection<DayOfWeek> ActiveDays
    {
        get
        {
            var selected = WeeklySchedules?.Where(s => s.IsSelected).Select(s => s.Day) ?? Enumerable.Empty<DayOfWeek>();
            return new ObservableCollection<DayOfWeek>(selected);
        }
    }

    public string ActiveDaysSummary
    {
        get
        {
            if (WeeklySchedules == null) return "بدون زمان‌بندی";

            var activeDays = WeeklySchedules.Where(s => s.IsSelected).ToList();
            if (activeDays.Count == 0) return "غیرفعال (روزی انتخاب نشده)";

            return string.Join(" | ", activeDays.Select(d => $"{d.DayTitle} ({d.ScheduledTime:hh\\:mm})"));
        }
    }

    public void NotifyDaysChanged()
    {
        OnPropertyChanged(nameof(ActiveDaysSummary));
        OnPropertyChanged(nameof(ActiveDays));
    }
}

public partial class UserProfile : ObservableObject
{
    [ObservableProperty]
    public partial string ProfileImagePath { get; set; } = "user_avatar.svg";

    [ObservableProperty]
    public partial int Points { get; set; } = 1250;

    [ObservableProperty]
    public partial int Level { get; set; } = 5;

    [ObservableProperty]
    public partial int CurrentStreak { get; set; } = 7;

    [ObservableProperty]
    public partial int HighestStreak { get; set; }

    [ObservableProperty]
    public partial int TotalTasksCompleted { get; set; }

    [ObservableProperty]
    public partial DateTime LastCompletionDate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LevelProgress))]
    public partial double CurrentXp { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LevelProgress))]
    public partial double RequiredXp { get; set; } = 100;

    [ObservableProperty]
    public partial ObservableCollection<string> UnlockedAchievements { get; set; } = [];

    public List<string> UnlockedAchievementIds { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> DiscoveredSecretAchievementIds { get; set; } = [];

    public double LevelProgress => RequiredXp > 0 ? Math.Clamp(CurrentXp / RequiredXp, 0.0, 1.0) : 0.0;

    // Daily four-checkpoint reminder configuration
    [ObservableProperty]
    public partial bool IsDailyReminderEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsMorningReminderEnabled { get; set; } = true;

    [ObservableProperty]
    public partial TimeSpan MorningReminderTime { get; set; } = new TimeSpan(7, 0, 0);

    [ObservableProperty]
    public partial bool IsNoonReminderEnabled { get; set; } = true;

    [ObservableProperty]
    public partial TimeSpan NoonReminderTime { get; set; } = new TimeSpan(12, 0, 0);

    [ObservableProperty]
    public partial bool IsEveningReminderEnabled { get; set; } = true;

    [ObservableProperty]
    public partial TimeSpan EveningReminderTime { get; set; } = new TimeSpan(17, 0, 0);

    [ObservableProperty]
    public partial bool IsNightReminderEnabled { get; set; } = true;

    [ObservableProperty]
    public partial TimeSpan NightReminderTime { get; set; } = new TimeSpan(21, 0, 0);

    // User-defined recurring custom schedules
    [ObservableProperty]
    public partial ObservableCollection<CustomReminder> CustomReminders { get; set; } = [];
    public string? AppPinCode { get; set; }
}