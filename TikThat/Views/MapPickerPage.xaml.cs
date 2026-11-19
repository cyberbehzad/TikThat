using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using TikThat.Services;
using TikThat.ViewModels;

namespace TikThat.Views;

public partial class MapPickerPage : ContentPage
{
    private readonly MapPickerViewModel _viewModel;
    private readonly PermissionService _permissionService;
    private bool _isMapReady;

    public MapPickerPage(MapPickerViewModel viewModel, PermissionService permissionService)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _permissionService = permissionService;

#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("AllowGeoAndScripts", (handler, view) =>
        {
            if (handler.PlatformView is Android.Webkit.WebView androidWebView)
            {
                androidWebView.Settings.JavaScriptEnabled = true;
                androidWebView.Settings.DomStorageEnabled = true;
                androidWebView.Settings.DatabaseEnabled = true;
                androidWebView.Settings.AllowFileAccess = true;
                androidWebView.Settings.AllowContentAccess = true;
                androidWebView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
                androidWebView.Settings.UserAgentString = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Mobile Safari/537.36 TikThat/1.0";
                androidWebView.SetLayerType(Android.Views.LayerType.Hardware, null);
            }
        });
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckConnectivityAndLoadMapAsync();
    }

    private async Task CheckConnectivityAndLoadMapAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            OfflineBorder.IsVisible = true;
            MapWebView.IsVisible = false;
            await DisplayAlert("خطای اتصال", "برای بارگذاری نقشه نیاز به اتصال اینترنت دارید.", "باشه");
            return;
        }

        OfflineBorder.IsVisible = false;
        MapWebView.IsVisible = true;
        await LoadMapHtmlAsync();
    }

    private async void OnRetryClicked(object? sender, EventArgs e)
    {
        await CheckConnectivityAndLoadMapAsync();
    }

    private async Task LoadMapHtmlAsync()
    {
        try
        {
            // Stream raw HTML from package assets to avoid WinUI path resolution failures
            using var stream = await FileSystem.OpenAppPackageFileAsync("map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            _isMapReady = false;

            // Remove BaseUrl to prevent cross-origin/CORS security assertion crashes in WebView2
            MapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MapPicker] Raw package loading faulted: {ex.Message}");
            await DisplayAlert("خطا در بارگذاری نقشه", "فایل نقشه در بسته برنامه بارگذاری نشد.", "باشه");
        }
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success)
        {
            _isMapReady = true;
        }
    }

    private async void OnHeaderMyLocationClicked(object? sender, EventArgs e)
    {
        await GoToUserCurrentLocationAsync();
    }

    private async Task GoToUserCurrentLocationAsync()
    {
#if !WINDOWS
        bool hasPermission = await _permissionService.CheckAndRequestLocationPermissionAsync();
        if (!hasPermission)
            return;
#endif

        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (location != null)
            {
                string lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
                string lng = location.Longitude.ToString(CultureInfo.InvariantCulture);

                if (_isMapReady)
                {
                    await MapWebView.EvaluateJavaScriptAsync($"panToCurrentLocation({lat}, {lng});");
                }
            }
            else
            {
                await DisplayAlert("خطای موقعیت", "موقعیت فعلی یافت نشد. لطفا مطمئن شوید GPS دستگاه روشن است.", "باشه");
            }
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            Debug.WriteLine($"[Geolocation] Failure: {ex.Message}");
            await DisplayAlert("خطا در GPS", "امکان دریافت مختصات فعلی در این پلتفرم وجود ندارد.", "باشه");
        }
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Url)) return;

        // Custom protocol routing for live position trigger
        if (e.Url.StartsWith("tikthat://getmyposition", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            await GoToUserCurrentLocationAsync();
            return;
        }

        // Custom protocol routing for final coordinate pickup
        if (e.Url.StartsWith("tikthat://location", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;

            try
            {
                var uri = new Uri(e.Url);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                string? latStr = queryParams["lat"];
                string? lngStr = queryParams["lng"];

                if (!string.IsNullOrEmpty(latStr) &&
                    !string.IsNullOrEmpty(lngStr) &&
                    double.TryParse(latStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lngStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
                {
                    await _viewModel.LocationSelectedAsync(lat, lng);
                }
                else
                {
                    await DisplayAlert("خطا", "مختصات انتخاب شده معتبر نیست.", "باشه");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MapNavigating] Parsing failure: {ex.Message}");
                await DisplayAlert("خطا", "پردازش موقعیت مکانی با خطا مواجه شد.", "باشه");
            }
        }
    }
}