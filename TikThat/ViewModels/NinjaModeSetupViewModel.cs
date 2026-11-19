using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using TikThat.Models;
using TikThat.Views;

namespace TikThat.ViewModels;

public partial class NinjaModeSetupViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaskTitle))]
    public partial TodoItem? TaskToFocus { get; set; }

    public string TaskTitle => TaskToFocus?.Text ?? "تسکی انتخاب نشده";

    [ObservableProperty]
    public partial int SelectedTime { get; set; } = 25;

    [ObservableProperty]
    public partial bool IsTransitioning { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("TaskToFocus", out var taskObj) && taskObj is TodoItem task)
        {
            TaskToFocus = task;
        }
    }

    [RelayCommand]
    private void SelectTime(string time)
    {
        if (int.TryParse(time, out int minutes))
        {
            SelectedTime = minutes;
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

    [RelayCommand]
    private async Task StartNinjaSession()
    {
        if (Shell.Current is null) return;

        if (TaskToFocus is null)
        {
            await Shell.Current.DisplayAlert("خطا", "تسکی برای تمرکز انتخاب نشده است.", "باشه");
            return;
        }

        if (SelectedTime <= 0)
        {
            await Shell.Current.DisplayAlert("خطا", "لطفاً یک زمان معتبر انتخاب کنید.", "باشه");
            return;
        }

        IsTransitioning = true;
        await Task.Delay(1200); // Allow ink transition animation to complete

        var navigationParameters = new Dictionary<string, object>
        {
            { "TaskToFocus", TaskToFocus },
            { "DurationInMinutes", SelectedTime }
        };

        await Shell.Current.GoToAsync(nameof(NinjaFocusPage), navigationParameters);

        await Task.Delay(500);
        IsTransitioning = false;
    }
}