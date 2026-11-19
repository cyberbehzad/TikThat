namespace TikThat.Services;

public interface INativeNotificationService
{
    Task<bool> RequestPermissionAsync();
    Task ShowAsync(string title, string message, int? notificationId = null);
}