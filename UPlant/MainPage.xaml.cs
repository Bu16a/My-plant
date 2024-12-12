using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using UPlant.Domain.Interfaces;

namespace UPlant;

public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public MainPage(IServiceProvider serviceProvider, IAuthService authService, INavigationService navigationService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _authService = authService;
        _navigationService = navigationService;
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
                await _navigationService.NavigateToAsync<ChoosePlantPage>(photo);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сделать фото: {ex.Message}", "OK");
        }
    }

    private async void OnMyPlantsClicked(object sender, EventArgs e)
    {
        await _navigationService.SetMainPageAsync<MyPlantsPage>();
    }

    private void OnPhotosClicked(object sender, EventArgs e) { }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _authService.LogoutAsync();
        await _navigationService.SetMainPageAsync<RegisterPage>();
    }
}