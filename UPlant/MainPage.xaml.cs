using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace UPlant;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
                await Navigation.PushAsync(new ChoosePlantPage(new() { "Ромашка", "Василёк", "Роза" }));
            //await Navigation.PushAsync(new ChoosePlantPage(await PlantDB.GetPossiblePlantsAsync(await photo.OpenReadAsync())));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сделать фото: {ex.Message}", "OK");
        }
    }

    private async void OnMyPlantsClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new NavigationPage(new MyPlantsPage());
    }

    private void OnPhotosClicked(object sender, EventArgs e) { }

    private async void OnLogoutClicked(object sender, EventArgs e) => await FirebaseApi.Logout();
}