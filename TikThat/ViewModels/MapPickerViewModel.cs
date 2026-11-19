using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TikThat.Messages;

namespace TikThat.ViewModels;

public partial class MapPickerViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public MapPickerViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public async Task LocationSelectedAsync(double latitude, double longitude)
    {
        string locationName = $"موقعیت ({latitude:F3}, {longitude:F3})";

        _messenger.Send(new LocationPickedMessage(latitude, longitude, locationName));

        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}