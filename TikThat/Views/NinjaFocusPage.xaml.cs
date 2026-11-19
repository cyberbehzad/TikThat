using TikThat.ViewModels;

namespace TikThat.Views;

public partial class NinjaFocusPage : ContentPage
{
    private readonly NinjaFocusViewModel _viewModel;

    public NinjaFocusPage(NinjaFocusViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }

    // هدایت دکمه Back فیزیکی گوشی به کامند انصراف نینجا
    protected override bool OnBackButtonPressed()
    {
        if (_viewModel.AbortSessionCommand.CanExecute(null))
        {
            _viewModel.AbortSessionCommand.Execute(null);
        }
        return true;
    }
}