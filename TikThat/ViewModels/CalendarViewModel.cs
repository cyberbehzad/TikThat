using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using TikThat.Models;

namespace TikThat.ViewModels;

[QueryProperty(nameof(CurrentDate), "CurrentDate")]
[QueryProperty(nameof(TaskDates), "TaskDates")]
public partial class CalendarViewModel : ObservableObject
{
    [ObservableProperty]
    public partial DateTime CurrentDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial List<DateTime> TaskDates { get; set; } = [];

    [ObservableProperty]
    public partial string CurrentMonthYearText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MonthImageSource { get; set; } = "farvardin.jpg";

    [ObservableProperty]
    public partial string SeasonalAnimationSource { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<CalendarDay> Days { get; set; } = [];

    public CalendarViewModel()
    {
        CurrentDate = DateTime.Today;
        GenerateCalendar();
        UpdateMonthAssets();
    }

    partial void OnCurrentDateChanged(DateTime value)
    {
        GenerateCalendar();
        UpdateMonthAssets();
    }

    partial void OnTaskDatesChanged(List<DateTime> value)
    {
        GenerateCalendar();
    }

    private void UpdateMonthAssets()
    {
        var persianCalendar = new PersianCalendar();
        var shamsiMonth = persianCalendar.GetMonth(CurrentDate);

        MonthImageSource = shamsiMonth switch
        {
            1 => "farvardin.jpg",
            2 => "ordibehesht.jpg",
            3 => "khordad.jpg",
            4 => "tir.jpg",
            5 => "mordad.jpg",
            6 => "shahrivar.jpg",
            7 => "mehr.jpg",
            8 => "aban.jpg",
            9 => "azar.jpg",
            10 => "dey.jpg",
            11 => "bahman.jpg",
            12 => "esfand.jpg",
            _ => "farvardin.jpg"
        };

        if (shamsiMonth >= 1 && shamsiMonth <= 3)
            SeasonalAnimationSource = "season_spring.json";
        else if (shamsiMonth >= 4 && shamsiMonth <= 6)
            SeasonalAnimationSource = "season_summer.json";
        else if (shamsiMonth >= 7 && shamsiMonth <= 9)
            SeasonalAnimationSource = "season_autumn.json";
        else
            SeasonalAnimationSource = "season_winter.json";
    }

    private void GenerateCalendar()
    {
        Days.Clear();
        var persianCalendar = new PersianCalendar();
        var year = persianCalendar.GetYear(CurrentDate);
        var month = persianCalendar.GetMonth(CurrentDate);

        CurrentMonthYearText = $"{new CustomShamsiDate(CurrentDate).MonthName} {year}";

        var firstDayOfMonth = persianCalendar.ToDateTime(year, month, 1, 0, 0, 0, 0);
        int daysInMonth = persianCalendar.GetDaysInMonth(year, month);

        DayOfWeek dow = persianCalendar.GetDayOfWeek(firstDayOfMonth);
        int blankDaysCount = ((int)dow + 1) % 7;

        for (int i = 0; i < blankDaysCount; i++)
        {
            Days.Add(new CalendarDay { IsOtherMonth = true });
        }

        for (int i = 1; i <= daysInMonth; i++)
        {
            var date = persianCalendar.ToDateTime(year, month, i, 0, 0, 0, 0);
            Days.Add(new CalendarDay
            {
                Day = i,
                Date = date,
                IsToday = date.Date == DateTime.Today,
                HasTask = TaskDates.Contains(date.Date)
            });
        }
    }

    [RelayCommand]
    private void ChangeMonth(string direction)
    {
        var persianCalendar = new PersianCalendar();
        if (direction == "next")
        {
            CurrentDate = persianCalendar.AddMonths(CurrentDate, 1);
        }
        else if (direction == "prev")
        {
            CurrentDate = persianCalendar.AddMonths(CurrentDate, -1);
        }
    }

    [RelayCommand]
    private async Task SelectDate(CalendarDay? day)
    {
        if (day is null || day.IsOtherMonth)
            return;

        var navigationParameters = new Dictionary<string, object>
        {
            { "SelectedDate", day.Date }
        };

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..", navigationParameters);
        }
    }

    [RelayCommand]
    private async Task GoBack()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}