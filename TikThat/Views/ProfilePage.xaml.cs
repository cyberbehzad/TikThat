using System;
using Microsoft.Maui.Controls;
using TikThat.ViewModels;

namespace TikThat.Views;

/// <summary>
/// Represents the user profile view, responsible for orchestrating profile state,
/// user achievements, and custom notification scheduler configurations.
/// </summary>
public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    /// <summary>
    /// Lifecycle hook triggered when the page becomes visible.
    /// Ensures user metrics and localized achievement states are refreshed.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeProfileData();
    }
}