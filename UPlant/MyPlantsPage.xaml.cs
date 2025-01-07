using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using UPlant.Domain.Interfaces;
using UPlant.Domain.Models;

namespace UPlant;

public partial class MyPlantsPage : ContentPage
{
    private readonly IPlantRepository _plantRepository;
    private readonly INavigationService _navigationService;

    public ObservableCollection<Plant> Plants { get; set; } = new();

    public MyPlantsPage(IPlantRepository plantRepository, INavigationService navigationService)
    {
        _plantRepository = plantRepository;
        _navigationService = navigationService;
        InitializeComponent();
        BindingContext = this;
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
        var plants = await _plantRepository.GetAllPlantsAsync();

        foreach (var plant in plants)
        {
            Plants.Add(plant);
            _ = Task.Run(async () =>
            {
                var imagePath = await _plantRepository.GetImagePathAsync(plant.Id);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    plant.SetImagePath(imagePath);
                });
            });
        }
    }

    private async void OnPlantSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedPlant = e.CurrentSelection[0] as Plant;
            await Navigation.PushAsync(new PlantSettingsPage(selectedPlant, _plantRepository));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private async void OnMyPlantsClicked(object sender, TappedEventArgs e)
    {
        // Already on MyPlants page, no need to navigate
    }

    private async void OnPhotosClicked(object sender, TappedEventArgs e)
    {
        await _navigationService.SetMainPageAsync<MainPage>();
    }
}