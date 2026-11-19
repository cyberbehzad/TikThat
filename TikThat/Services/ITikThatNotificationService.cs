using System.Collections.Generic;
using System.Threading.Tasks;
using TikThat.Models;

namespace TikThat.Services;

public interface ITikThatNotificationService
{
    Task<bool> RequestPermissionAsync();
    Task ShowNotification(string title, string body, int id = 0);
    Task ScheduleTaskReminderAsync(TodoItem task);
    Task CancelTaskReminderAsync(TodoItem task);
    Task RescheduleDailyRemindersAsync(UserProfile profile, IEnumerable<TodoItem> todayTasks);
}