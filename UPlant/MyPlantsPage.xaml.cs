using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace UPlant;

public partial class MyPlantsPage : ContentPage
{
    public ObservableCollection<Plant> Plants { get; set; } = new();

    public MyPlantsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPlantsAsync();
        PhotoCollectionView.ItemsSource = Plants;
    }

    private async Task LoadPlantsAsync()
    {
        Plants.Clear();

        foreach (var plant in PlantDB.Plants)
        {
            Plants.Add(plant);
            _ = Task.Run(async () =>
            {
                await plant.LoadImagePathAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    plant.IsImageLoading = false;
                    plant.IsImageLoaded = true;
                });
            });
        }
    }

    private async void OnPlantSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            await Navigation.PushAsync(new PlantSettingsPage(e.CurrentSelection[0] as Plant));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private void OnMyPlantsClicked(object sender, EventArgs e) { }

    private async void OnPhotosClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}