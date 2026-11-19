using TikThat.Views;

namespace TikThat;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(MapPickerPage), typeof(MapPickerPage));
        Routing.RegisterRoute(nameof(NinjaModeSetupPage), typeof(NinjaModeSetupPage));
        Routing.RegisterRoute(nameof(NinjaFocusPage), typeof(NinjaFocusPage));
    }
}