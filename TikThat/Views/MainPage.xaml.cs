using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using SkiaSharp.Extended.UI.Controls;
using TikThat.ViewModels;

namespace TikThat.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private CancellationTokenSource? _animationCts;
    private CancellationTokenSource? _confettiCts;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // اشتراک در رویداد درخواست پخش انیمیشن های موقت تسک از سمت ViewModel
        _viewModel.PlayAnimationRequested += OnPlayAnimationRequested;

        // گوش دادن به وضعیت تکمیل شدن تمام تسک های روز برای پخش تک شات جشن
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();

        // اگر هنگام ورود به صفحه تمام تسک ها تمام شده بودند، یک بار اجرا شود
        if (_viewModel.AreAllTasksDone)
        {
            TriggerConfettiOnce();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelCurrentAnimation();
        CancelConfettiAnimation();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.AreAllTasksDone))
        {
            if (_viewModel.AreAllTasksDone)
            {
                TriggerConfettiOnce();
            }
            else
            {
                // در صورت اضافه شدن تسک جدید یا برگشت تسک از تیک، جشن بلافاصله مخفی شود
                CancelConfettiAnimation();
            }
        }
    }

    private void OnPlayAnimationRequested(string animationFileName)
    {
        // فراخوانی امن در ترد اصلی رابط کاربری
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await PlayOneShotAnimationAsync(animationFileName);
        });
    }

    public async Task PlayOneShotAnimationAsync(string jsonFileName)
    {
        // لغو اجرای انیمیشن قبلی در صورت تپ های سریع و متوالی
        CancelCurrentAnimation();
        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;

        try
        {
            // ۱. خاموش کردن کامل و ریست موقعیت پخش به فریم اول
            DynamicLottieView.IsAnimationEnabled = false;
            DynamicLottieView.Progress = TimeSpan.Zero;
            DynamicLottieView.RepeatCount = 0;

            // ۲. مقداردهی معتبر سورس انیمیشن با نمونه سازی صریح شیء لاتی اسکایا
            DynamicLottieView.Source = new SKFileLottieImageSource
            {
                File = jsonFileName
            };

            // ۳. نمایش کانتینر لایه رویی و استارت پخش
            OverlayAnimationContainer.IsVisible = true;
            DynamicLottieView.IsAnimationEnabled = true;

            // ۴. مهلت زمان اجرای دقیق انیمیشن (۱.۵ ثانیه)
            await Task.Delay(1500, token);

            // ۵. خاموش کردن و پنهان سازی کنترل
            DynamicLottieView.IsAnimationEnabled = false;
            OverlayAnimationContainer.IsVisible = false;
            DynamicLottieView.Progress = TimeSpan.Zero;
        }
        catch (TaskCanceledException)
        {
            // نادیده گرفتن لغو در اثر تپ تسک جدید
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Lottie Playback Error]: {ex.Message}");
            OverlayAnimationContainer.IsVisible = false;
        }
    }

    private void TriggerConfettiOnce()
    {
        CancelConfettiAnimation();
        _confettiCts = new CancellationTokenSource();
        var token = _confettiCts.Token;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (AllDoneAnimationView == null) return;

                // ۱. مقداردهی قطعی سورس و ریست فریم
                AllDoneAnimationView.IsAnimationEnabled = false;
                AllDoneAnimationView.Progress = TimeSpan.Zero;
                AllDoneAnimationView.RepeatCount = 0;
                AllDoneAnimationView.Source = new SKFileLottieImageSource
                {
                    File = "all_done_confetti.json"
                };

                // ۲. نمایش و آغاز پخش
                AllDoneAnimationView.IsVisible = true;
                AllDoneAnimationView.IsAnimationEnabled = true;

                // ۳. مهلت زمان اجرای تک شات (۲.۵ ثانیه)
                await Task.Delay(2500, token);

                // ۴. توقف کامل و ناپدیدسازی برای جلوگیری از قفل ماندن فریم ها
                AllDoneAnimationView.IsAnimationEnabled = false;
                AllDoneAnimationView.IsVisible = false;
                AllDoneAnimationView.Progress = TimeSpan.Zero;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Confetti Animation Error]: {ex.Message}");
                if (AllDoneAnimationView != null)
                {
                    AllDoneAnimationView.IsVisible = false;
                }
            }
        });
    }

    private void CancelCurrentAnimation()
    {
        if (_animationCts != null)
        {
            _animationCts.Cancel();
            _animationCts.Dispose();
            _animationCts = null;
        }
    }

    private void CancelConfettiAnimation()
    {
        if (_confettiCts != null)
        {
            _confettiCts.Cancel();
            _confettiCts.Dispose();
            _confettiCts = null;
        }

        if (AllDoneAnimationView != null)
        {
            AllDoneAnimationView.IsAnimationEnabled = false;
            AllDoneAnimationView.IsVisible = false;
            AllDoneAnimationView.Progress = TimeSpan.Zero;
        }
    }
}