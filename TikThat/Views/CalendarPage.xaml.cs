using TikThat.ViewModels;

namespace TikThat.Views;

public partial class CalendarPage : ContentPage
{
    public CalendarPage()
    {
        InitializeComponent();
        BindingContext = new CalendarViewModel();
    }
}
