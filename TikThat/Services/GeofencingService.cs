using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace TikThat.Services;

public class DefaultNotificationService : INativeNotificationService
{
    public Task<bool> RequestPermissionAsync()
    {
        return Task.FromResult(true);
    }

    public Task ShowAsync(string title, string message, int? notificationId = null)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
            {
                try
                {
                    await page.DisplayAlertAsync(title, message, "باشه");
                }
                catch
                {
                    Debug.WriteLine($"{title}: {message}");
                }
            }
            else
            {
                Debug.WriteLine($"{title}: {message}");
            }
        });

        return Task.CompletedTask;
    }
}