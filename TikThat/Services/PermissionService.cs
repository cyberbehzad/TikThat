using Microsoft.Maui.ApplicationModel;
using Plugin.LocalNotification;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MauiPermissions = Microsoft.Maui.ApplicationModel.Permissions;

namespace TikThat.Services;

public class PermissionService
{
    // Check and request push/local notification permission
    public async Task<bool> CheckAndRequestNotificationPermissionAsync()
    {
#if WINDOWS
        // Windows toast notifications run via WinRT/Task Scheduler without runtime permission prompts
        return await Task.FromResult(true);
#else
        try
        {
            bool isGranted = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            if (!isGranted)
            {
                isGranted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            if (!isGranted && Shell.Current != null)
            {
                bool openSettings = await Shell.Current.DisplayAlert(
                    "دسترسی اعلان‌ها",
                    "برای اینکه زمان رسیدن به محل کار، خرید یا دانشگاه، تیک دت بتواند به شما یادآوری ارسال کند، به این مجوز نیاز داریم. مایلید آن را در تنظیمات فعال کنید؟",
                    "باز کردن تنظیمات",
                    "فعلاً نه");

                if (openSettings)
                {
                    AppInfo.Current.ShowSettingsUI();
                }
                return false;
            }

            return isGranted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Notification Permission Error]: {ex.Message}");
            return false;
        }
#endif
    }

    // Check and request media library permission for profile avatars
    public async Task<bool> CheckAndRequestPhotoPermissionAsync()
    {
#if WINDOWS
        // Windows uses FileOpenPicker and does not require mobile gallery permissions
        return await Task.FromResult(true);
#else
        var status = await MauiPermissions.CheckStatusAsync<MauiPermissions.Photos>();

        if (status != PermissionStatus.Granted)
        {
            status = await MauiPermissions.RequestAsync<MauiPermissions.Photos>();
        }

        if (status != PermissionStatus.Granted)
        {
            if (Shell.Current != null)
            {
                bool openSettings = await Shell.Current.DisplayAlert(
                    "دسترسی به تصاویر",
                    "این دسترسی صرفاً برای انتخاب عکس پروفایل شخصی شما از گالری استفاده می‌شود و هیچ فایلی به سرور ارسال نخواهد شد. آیا مایلید از تنظیمات فعال کنید؟",
                    "رفتن به تنظیمات",
                    "انصراف");

                if (openSettings)
                {
                    AppInfo.Current.ShowSettingsUI();
                }
            }
            return false;
        }

        return true;
#endif
    }

    // Check and request foreground precise location access
    public async Task<bool> CheckAndRequestLocationPermissionAsync()
    {
#if WINDOWS
        try
        {
            var status = await MauiPermissions.CheckStatusAsync<MauiPermissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
                return true;

            // Prompt user if system-wide Windows Location Services are disabled
            status = await MauiPermissions.RequestAsync<MauiPermissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
                return true;

            if (Shell.Current != null)
            {
                bool openWinSettings = await Shell.Current.DisplayAlert(
                    "سرویس موقعیت مکانی ویندوز",
                    "سرویس لوکیشن در تنظیمات ویندوز شما خاموش است. مایلید بخش Location ویندوز را باز کنید تا روشن شود؟\n(در صورت انصراف، نقشه با موقعیت پیش‌فرض باز می‌شود)",
                    "تنظیمات ویندوز",
                    "ادامه با موقعیت پیش‌فرض");

                if (openWinSettings)
                {
                    await Launcher.Default.OpenAsync("ms-settings:privacy-location");
                }
            }
            
            // Allow map to open with fallback coordinates on Windows even if location access is denied
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Windows Location Error]: {ex.Message}");
            return true;
        }
#else
        var status = await MauiPermissions.CheckStatusAsync<MauiPermissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await MauiPermissions.RequestAsync<MauiPermissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            if (Shell.Current != null)
            {
                bool openSettings = await Shell.Current.DisplayAlert(
                    "دسترسی به مکان دقیق (GPS)",
                    "برای یافتن موقعیت فعلی شما و ثبت دقیق مقصد روی نقشه، دسترسی مکان لازم است.\n\n" +
                    "🔒 حریم خصوصی: موقعیت مکانی شما فقط و فقط داخل حافظه گوشی خودتان (دیتابیس آفلاین) نگهداری می‌شود و به هیچ سرور یا مقصدی مخابره نمی‌شود.\n\n" +
                    "آیا مایلید برای تعیین مکان تسک، این گزینه را فعال کنید؟",
                    "رفتن به تنظیمات",
                    "انصراف");

                if (openSettings)
                {
                    AppInfo.Current.ShowSettingsUI();
                }
            }
            return false;
        }

        return true;
#endif
    }

    // Check and request always-on background location access (Android)
    public async Task<bool> CheckAndRequestBackgroundLocationAsync()
    {
#if WINDOWS
        // Background location permission is not applicable on Windows desktop
        return await Task.FromResult(true);
#else
        bool hasForeground = await CheckAndRequestLocationPermissionAsync();
        if (!hasForeground)
            return false;

        var bgStatus = await MauiPermissions.CheckStatusAsync<MauiPermissions.LocationAlways>();

        if (bgStatus != PermissionStatus.Granted)
        {
            if (Shell.Current != null)
            {
                bool explain = await Shell.Current.DisplayAlert(
                    "تنظیم دسترسی همیشگی (بسیار مهم)",
                    "سیستم‌عامل اندروید برای مدیریت مصرف باتری، گزینه «همیشه مجاز باشد» را درون پنجره‌های سریع نمایش نمی‌دهد.\n\n" +
                    "برای اینکه حتی وقتی صفحه گوشی قفل است یا برنامه بسته است، با ورود به شعاع ۲۰۰ متری تسک اعلان یادآوری دریافت کنید:\n\n" +
                    "۱. دکمه «رفتن به تنظیمات» را لمس کنید.\n" +
                    "۲. وارد بخش «Permissions» (مجوزها) و سپس «Location» شوید.\n" +
                    "۳. گزینه «Allow all the time» (همیشه مجاز باشد) و تیک «Use precise location» را فعال کنید.\n\n" +
                    "🛡️ شفافیت و امنیت:\n" +
                    "تیک دت یک برنامه کاملاً لوکال و آفلاین است. موقعیت شما ردگیری یا مخابره نمی‌شود؛ سنسور داخلی گوشی صرفاً محدوده ۲۰۰ متری تسک را پایش می‌کند.",
                    "رفتن به تنظیمات",
                    "رد کردن");

                if (explain)
                {
                    AppInfo.Current.ShowSettingsUI();
                }
            }
            return false;
        }

        return true;
#endif
    }

    // Verify core permissions during application startup
    public async Task InitializeStartupPermissionsAsync()
    {
        await CheckAndRequestNotificationPermissionAsync();
    }

    private const string KeyDoNotShowGpsKeepAliveWarning = "DoNotShowGpsKeepAliveWarning";

    public async Task EnsureGpsKeepAliveAlertAsync()
    {
#if WINDOWS
        // Skip mobile GPS/battery keep-alive warning on desktop
        await Task.CompletedTask;
#else
        bool doNotShowAgain = Preferences.Default.Get(KeyDoNotShowGpsKeepAliveWarning, false);
        if (doNotShowAgain)
            return;

        if (Shell.Current != null)
        {
            bool rememberChoice = await Shell.Current.DisplayAlert(
                "📍 روشن ماندن موقعیت مکانی (GPS)",
                "برای اینکه تیک‌ دت بتواند زمان رسیدن به محل تسک به شما پیام یادآوری بدهد، سنسور موقعیت مکانی (GPS) گوشی باید روشن بماند.\n\n" +
                "در صورت خاموش کردن GPS از کنترل‌سنتر گوشی، هشدارهای مکانی صادر نخواهند شد.",
                "متوجه شدم (دیگر نشان نده)",
                "متوجه شدم");

            if (rememberChoice)
            {
                Preferences.Default.Set(KeyDoNotShowGpsKeepAliveWarning, true);
            }
        }
#endif
    }
}