using CommunityToolkit.Mvvm.ComponentModel;

namespace TikThat.Models;

public partial class CalendarDay : ObservableObject
{
    [ObservableProperty]
    public partial int Day { get; set; }

    [ObservableProperty]
    public partial int GregorianDay { get; set; }

    [ObservableProperty]
    public partial DateTime Date { get; set; }

    [ObservableProperty]
    public partial bool IsToday { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsOtherMonth { get; set; }

    [ObservableProperty]
    public partial bool HasTask { get; set; }
}