using Shiny.Locations;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using TikThat.Services;

namespace TikThat;

public class MyGeofenceDelegate : IGeofenceDelegate
{
    private readonly ITikThatNotificationService _notificationService;

    private static readonly ConcurrentDictionary<string, DateTime> _lastAlertTimes = new();
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromMinutes(10);

    public MyGeofenceDelegate(ITikThatNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
    {
        try
        {
            if (region == null || string.IsNullOrWhiteSpace(region.Identifier))
                return;

            string regionKey = $"{region.Identifier}_{newStatus}";

            
// Suppress duplicate notifications caused by GPS boundary jitter
            if (_lastAlertTimes.TryGetValue(regionKey, out var lastTime))
            {
                if (DateTime.UtcNow - lastTime < AlertCooldown)
                {
                    Debug.WriteLine($"[Geofence] Alert ignored due to cooldown for region: {region.Identifier}");
                    return;
                }
            }

            switch (newStatus)
            {
                case GeofenceState.Entered:
                    await _notificationService.ShowNotification(
                        "📍 یادآور مکانی TikThat",
                        $"شما به محدوده تسک «{region.Identifier}» رسیدید!"
                    );
                    _lastAlertTimes[regionKey] = DateTime.UtcNow;
                    break;

                case GeofenceState.Exited:
                    Debug.WriteLine($"[Geofence] Exited region: {region.Identifier}");
                    break;

                case GeofenceState.Unknown:
                default:
                    Debug.WriteLine($"[Geofence] Region {region.Identifier} state is unknown.");
                    break;
            }
        }
        catch (Exception ex)
        {
            
// Isolate exceptions to prevent crashing the Android background receiver
            Debug.WriteLine($"[Geofence Error] Exception in OnStatusChanged: {ex.Message}");
        }
    }
}