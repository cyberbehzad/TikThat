using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TikThat.Messages;

public class LocationPickedMessage : ValueChangedMessage<string>
{
    public double Latitude { get; }
    public double Longitude { get; }
    public string LocationName { get; }

    // Primary constructor with explicit coordinates and display name
    public LocationPickedMessage(double latitude, double longitude, string locationName)
        : base($"{{\"latitude\":{latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"longitude\":{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}")
    {
        Latitude = latitude;
        Longitude = longitude;
        LocationName = locationName;
    }

    // Legacy single-argument overload for backward compatibility
    public LocationPickedMessage(string value) : base(value)
    {
        LocationName = string.Empty;
    }
}