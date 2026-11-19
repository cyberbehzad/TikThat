#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;

namespace TikThat.Services;

public static class WindowsScheduledNotificationService
{
    public static void ScheduleTaskNotification(int taskId, string title, string body, DateTime notifyTime)
    {
        try
        {
            if (notifyTime <= DateTime.Now) return;

            string taskName = $"TikThat_Reminder_{taskId}";
            
            CancelNotification(taskId);

            string timeString = notifyTime.ToString("HH:mm");
            string dateString = notifyTime.ToString("yyyy/MM/dd");

            
// Invoke WinRT ToastNotificationManager via PowerShell for unpackaged scheduled alerts
            string safeTitle = title.Replace("\"", "`\"");
            string safeBody = body.Replace("\"", "`\"");
            
            string psScript = $"[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null; " +
                              $"$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02); " +
                              $"$textNodes = $template.GetElementsByTagName('text'); " +
                              $"$textNodes.Item(0).AppendChild($template.CreateTextNode('{safeTitle}')) > $null; " +
                              $"$textNodes.Item(1).AppendChild($template.CreateTextNode('{safeBody}')) > $null; " +
                              $"$toast = [Windows.UI.Notifications.ToastNotification]::new($template); " +
                              $"[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('TikThat').Show($toast);";

            string encodedScript = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(psScript));
            string schtasksArgs = $"/create /tn \"{taskName}\" /tr \"powershell -NoProfile -WindowStyle Hidden -EncodedCommand {encodedScript}\" /sc once /st {timeString} /sd {dateString} /f";

            ProcessStartInfo psi = new()
            {
                FileName = "schtasks.exe",
                Arguments = schtasksArgs,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Schedule Notification Error]: {ex.Message}");
        }
    }

    public static void CancelNotification(int taskId)
    {
        try
        {
            string taskName = $"TikThat_Reminder_{taskId}";
            ProcessStartInfo psi = new()
            {
                FileName = "schtasks.exe",
                Arguments = $"/delete /tn \"{taskName}\" /f",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi);
        }
        catch
        {
        }
    }
}
#endif