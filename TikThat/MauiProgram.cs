using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.LocalNotification;
using Shiny;
using Shiny.Locations;
using TikThat.Models;
using TikThat.Services;
using TikThat.ViewModels;
using TikThat.Views;

namespace TikThat;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseShiny()
            .UseSkiaSharp()
            .UseLocalNotification()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Vazirmatn-Regular.ttf", "Vazirmatn");
            });

        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        builder.Services.AddSingleton<DatabaseService>();

        builder.Services.AddGeofencing<TaskGeofenceDelegate>();

        builder.Services.AddSingleton<PermissionService>();
        builder.Services.AddSingleton<IUserProfileService, UserProfileService>();
        builder.Services.AddSingleton<UserProfileService>();
        builder.Services.AddSingleton<AchievementService>();
        builder.Services.AddSingleton<ITikThatNotificationService, NotificationService>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<CalendarPage>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();

        builder.Services.AddTransient<NinjaModeSetupViewModel>();
        builder.Services.AddTransient<NinjaModeSetupPage>();
        builder.Services.AddTransient<NinjaFocusViewModel>();
        builder.Services.AddTransient<NinjaFocusPage>();

        builder.Services.AddTransient<MapPickerViewModel>();
        builder.Services.AddTransient<MapPickerPage>();

        builder.Services.AddSingleton(sp => new System.Collections.ObjectModel.ObservableCollection<TodoItem>());

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}