using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TikThat.Models;
using TikThat.Services;

namespace TikThat.ViewModels;

[QueryProperty(nameof(TaskToFocus), "TaskToFocus")]
[QueryProperty(nameof(DurationInMinutes), "DurationInMinutes")]
public partial class NinjaFocusViewModel : ObservableObject
{
    private readonly IUserProfileService _profileService;

    public NinjaFocusViewModel(IUserProfileService profileService)
    {
        _profileService = profileService;
    }

    [ObservableProperty]
    public partial TodoItem? TaskToFocus { get; set; }

    [ObservableProperty]
    public partial int DurationInMinutes { get; set; } = 25;

    [ObservableProperty]
    public partial string TimerText { get; set; } = "25:00";

    [ObservableProperty]
    public partial double Progress { get; set; } = 1.0;

    [ObservableProperty]
    public partial bool IsCompletionAnimationVisible { get; set; }

    [ObservableProperty]
    public partial bool IsTransitioning { get; set; }

    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    [ObservableProperty]
    public partial string PauseButtonText { get; set; } = "توقف";

    private IDispatcherTimer? _timer;
    private TimeSpan _timeRemaining;
    private bool _isInitialized;
    private bool _isFinished;

    partial void OnDurationInMinutesChanged(int value)
    {
        if (value <= 0) value = 25;
        _timeRemaining = TimeSpan.FromMinutes(value);
        TimerText = _timeRemaining.ToString(@"mm\:ss");
        Progress = 1.0;
        _isInitialized = true;
    }

    public void OnAppearing()
    {
        if (_isFinished || IsPaused) return;
        StartSession();
    }

    public void OnDisappearing()
    {
        _timer?.Stop();
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        if (_timeRemaining.TotalSeconds > 1)
        {
            _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
            TimerText = _timeRemaining.ToString(@"mm\:ss");

            double totalSeconds = Math.Max(1, DurationInMinutes * 60.0);
            Progress = Math.Clamp(_timeRemaining.TotalSeconds / totalSeconds, 0.0, 1.0);
        }
        else
        {
            _timeRemaining = TimeSpan.Zero;
            TimerText = "00:00";
            Progress = 0.0;
            _timer?.Stop();
            await FailSessionAsync();
        }
    }

    [RelayCommand]
    public void StartSession()
    {
        if (_isFinished) return;

        if (!_isInitialized || _timeRemaining <= TimeSpan.Zero)
        {
            int minutes = DurationInMinutes > 0 ? DurationInMinutes : 25;
            _timeRemaining = TimeSpan.FromMinutes(minutes);
            TimerText = _timeRemaining.ToString(@"mm\:ss");
            Progress = 1.0;
            _isInitialized = true;
        }

        if (_timer is null)
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.GetForCurrentThread();
            if (dispatcher is not null)
            {
                _timer = dispatcher.CreateTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += OnTimerTick;
            }
        }

        IsPaused = false;
        PauseButtonText = "توقف";
        _timer?.Start();
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (_timer is null || _isFinished) return;

        IsPaused = !IsPaused;
        if (IsPaused)
        {
            _timer.Stop();
            PauseButtonText = "ادامه";
        }
        else
        {
            _timer.Start();
            PauseButtonText = "توقف";
        }
    }

    [RelayCommand]
    private async Task AbortSessionAsync()
    {
        if (_isFinished || Shell.Current is null) return;

        _timer?.Stop();

        bool confirmed = await Shell.Current.DisplayAlert(
            "انصراف از ماموریت",
            "آیا مطمئن هستید؟ تمرکز شما لغو خواهد شد.",
            "بله، تسلیم می‌شوم", "ادامه تمرکز");

        if (confirmed)
        {
            _isFinished = true;
            CleanUpTimer();
            await CloseAsync();
        }
        else if (!IsPaused)
        {
            _timer?.Start();
        }
    }

    [RelayCommand]
    private async Task CompleteSessionAsync()
    {
        if (_isFinished) return;
        _isFinished = true;
        CleanUpTimer();

        if (TaskToFocus is not null && !TaskToFocus.IsCompleted)
        {
            TaskToFocus.IsCompleted = true;

            try
            {
                var profile = await _profileService.LoadProfileAsync();
                profile.Points += 15;
                profile.TotalTasksCompleted++;
                await _profileService.SaveProfileAsync(profile);
            }
            catch
            {
            }
        }

        IsCompletionAnimationVisible = true;
        await Task.Delay(1800);
        IsCompletionAnimationVisible = false;

        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert(
                "پیروزی نینجا!",
                "ماموریت با موفقیت انجام شد! ۱۵ امتیاز نینجا دریافت کردید.",
                "بسیار عالی");
        }

        await CloseAsync();
    }

    private async Task FailSessionAsync()
    {
        if (_isFinished) return;
        _isFinished = true;
        CleanUpTimer();

        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert(
                "زمان به پایان رسید!",
                "فرصت شما تمام شد. برای کسب امتیاز باید قبل از پایان تایمر تسک را کامل کنید.",
                "باشه");
        }

        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        IsTransitioning = true;
        await Task.Delay(750);

        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync("..");
        }

        IsTransitioning = false;
    }

    private void CleanUpTimer()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }
}