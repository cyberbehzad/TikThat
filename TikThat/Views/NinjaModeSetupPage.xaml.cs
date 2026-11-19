using TikThat.ViewModels;

namespace TikThat.Views;

public partial class NinjaModeSetupPage : ContentPage
{
    public NinjaModeSetupPage(NinjaModeSetupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}