using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace TikThat.Models;

[Table("Tasks")]
public partial class TodoItem : ObservableObject
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Indexed]
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    [ObservableProperty]
    public partial bool IsSwipeHintVisible { get; set; }

    [Indexed]
    [ObservableProperty]
    public partial DateTime DueDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial bool IsLocked { get; set; }

    // Geofencing and location-based reminder metadata
    [Indexed]
    [ObservableProperty]
    public partial bool HasLocationReminder { get; set; }

    [ObservableProperty]
    public partial double Latitude { get; set; }

    [ObservableProperty]
    public partial double Longitude { get; set; }

    [ObservableProperty]
    public partial string LocationName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? LastLocationNotificationAt { get; set; }

    // Time-based reminder scheduling properties
    [Indexed]
    [ObservableProperty]
    public partial bool HasTimeReminder { get; set; }

    [ObservableProperty]
    public partial TimeSpan? ReminderTime { get; set; }

    [ObservableProperty]
    public partial int NotificationId { get; set; }

    // Read-only formatted reminder timestamp for XAML binding
    [Ignore]
    public string FormattedReminderTime => ReminderTime.HasValue
        ? $"{ReminderTime.Value.Hours:D2}:{ReminderTime.Value.Minutes:D2}"
        : string.Empty;
}