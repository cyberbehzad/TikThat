using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;
using Shiny.Locations;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TikThat.Models;

namespace TikThat.Services;

public class TaskGeofenceDelegate : IGeofenceDelegate
{
    private readonly DatabaseService _databaseService;

    public TaskGeofenceDelegate(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
    {
        // Trigger only when entering the geofence perimeter
        if (newStatus != GeofenceState.Entered)
            return;

        try
        {
            // Resolve matching active location task from local SQLite store
            var locationTasks = await _databaseService.GetLocationTasksAsync();
            var matchedTask = locationTasks.FirstOrDefault(t => t.Id == region.Identifier || t.Text == region.Identifier);

            // Skip if task no longer exists or was already completed manually
            if (matchedTask == null || matchedTask.IsCompleted)
                return;

            var now = DateTime.Now;

            // Suppress notification if scheduled time reminder is more than 15 minutes away
            if (matchedTask.HasTimeReminder && matchedTask.ReminderTime.HasValue)
            {
                var targetDateTime = matchedTask.DueDate.Date + matchedTask.ReminderTime.Value;
                if (targetDateTime > now.AddMinutes(15))
                {
                    return;
                }
            }

            // Throttle notifications to prevent boundary-jitter spam within a 30-minute cooldown
            if (matchedTask.LastLocationNotificationAt.HasValue &&
                (now - matchedTask.LastLocationNotificationAt.Value).TotalMinutes < 30)
            {
                return;
            }

            // Persist last trigger timestamp to avoid repeated alerts along the boundary
            matchedTask.LastLocationNotificationAt = now;
            await _databaseService.SaveTaskAsync(matchedTask);

            // Dispatch local alert without mutating task completion state
            var notification = new NotificationRequest
            {
                NotificationId = Random.Shared.Next(1000, 9999),
                Title = "📍 به محل تسک رسیدید!",
                Description = $"یادآوری: «{matchedTask.Text}»\n(پس از اتمام کار، تیک انجام تسک را در برنامه ثبت کنید)",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(1)
                },
                Android = new AndroidOptions
                {
                    ChannelId = "tikthat_geofence_channel",
                    Priority = AndroidPriority.High
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskGeofenceDelegate Error]: {ex.Message}");
        }
    }
}