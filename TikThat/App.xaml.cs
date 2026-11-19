using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using TikThat.Services;

#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
#endif

namespace TikThat;

/// <summary>
/// Root application lifecycle orchestrator handling cross-platform initialization,
/// state persistence, window lifecycle management, system tray integration, and OS-level notification activators.
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly PermissionService _permissionService;

#if WINDOWS
    /// <summary>
    /// Holds a global static reference to the active WinUI 3 AppWindow for visibility and tray orchestration.
    /// </summary>
    public static Microsoft.UI.Windowing.AppWindow? MainWindow { get; private set; }
#endif

    public App(PermissionService permissionService)
    {
        InitializeComponent();
        _permissionService = permissionService;

#if WINDOWS
        RegisterAppUserModelId();
#endif
    }

    /// <summary>
    /// Configures the primary presentation shell and registers desktop window hooks,
    /// intercepting termination events to preserve background timers inside the Windows System Tray.
    /// </summary>
    protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState)
    {
        UserAppTheme = AppTheme.Dark;
        var window = new Microsoft.Maui.Controls.Window(new AppShell());

#if WINDOWS
        window.Created += (s, e) =>
        {
            var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow != null)
            {
                MainWindow = nativeWindow.AppWindow;

                // Instantiate and register the native notification area (System Tray) icon
                WindowsTrayService.InitializeTray();

                // Intercept window close to suppress process termination, hiding UI to the notification area
                MainWindow.Closing += (sender, args) =>
                {
                    args.Cancel = true;
                    MainWindow.Hide();
                };
            }
        };

        window.Destroying += (s, e) =>
        {
            // Clean up Win32 tray resources upon definitive process shutdown
            WindowsTrayService.RemoveTray();
        };
#endif

        return window;
    }

    /// <summary>
    /// Global application lifecycle hook executed when the environment transitions to active state.
    /// Restores persisted tasks and recurring daily/weekly schedules back into the memory scheduler queue.
    /// </summary>
    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            await _permissionService.InitializeStartupPermissionsAsync();

#if WINDOWS
            var dbService = Handler?.MauiContext?.Services.GetService<DatabaseService>();
            var notifService = Handler?.MauiContext?.Services.GetService<ITikThatNotificationService>();
            var profileService = Handler?.MauiContext?.Services.GetService<IUserProfileService>() 
                                 ?? Handler?.MauiContext?.Services.GetService<UserProfileService>();

            if (dbService != null && notifService != null)
            {
                // 1. Re-hydrate pending task-bound reminders
                var allTasks = await dbService.GetAllTasksAsync();
                var pendingTasks = allTasks.Where(t => !t.IsCompleted && t.HasTimeReminder && t.ReminderTime.HasValue);

                foreach (var task in pendingTasks)
                {
                    await notifService.ScheduleTaskReminderAsync(task);
                }

                // 2. Re-hydrate profile-based daily checkpoints and weekly custom schedules
                if (profileService != null)
                {
                    var profile = await profileService.LoadProfileAsync();
                    if (profile != null)
                    {
                        await notifService.RescheduleDailyRemindersAsync(profile, allTasks);
                    }
                }
            }
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Startup Hydration Error]: {ex.Message}");
        }
    }

#if WINDOWS
    /// <summary>
    /// Explicitly registers AppUserModelID and out-of-proc COM activator required for unpackaged toasts.
    /// </summary>
    private static void RegisterAppUserModelId()
    {
        try
        {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<TikThatNotificationActivator>("TikThat.Desktop.App");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AUMID Initialization Exception]: {ex.Message}");
        }
    }
#endif
}

#if WINDOWS
/// <summary>
/// Native COM activator interface handling user action routing when a desktop notification banner is clicked.
/// </summary>
[ClassInterface(ClassInterfaceType.None)]
[ComSourceInterfaces(typeof(INotificationActivationCallback))]
[Guid("4A1B2C3D-E4F5-6789-ABCD-EF0123456789"), ComVisible(true)]
public class TikThatNotificationActivator : NotificationActivator
{
    public override void OnActivated(string arguments, NotificationUserInput userInput, string appUserModelId)
    {
        Debug.WriteLine($"[Notification Interaction Payload]: {arguments}");

        // Restore window visibility and bring shell to foreground upon user toast interaction
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            App.MainWindow?.Show();
        });
    }
}
#endif