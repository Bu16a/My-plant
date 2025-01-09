using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UPlant.Domain.Interfaces;
using UPlant.Domain.Models;

namespace UPlant;

public partial class MyPlantsPage : ContentPage
{
    private readonly IPlantRepository _plantRepository;
    private readonly INavigationService _navigationService;

    public ObservableCollection<Plant> Plants { get; set; } = new();
    
    public ICommand PlantSelectedCommand { get; private set; }

    public MyPlantsPage(IPlantRepository plantRepository, INavigationService navigationService)
    {
        _plantRepository = plantRepository;
        _navigationService = navigationService;
        InitializeComponent();
        BindingContext = this;
        
        PlantSelectedCommand = new Command<Plant>(async (plant) =>
        {
            if (plant != null)
            {
                await Navigation.PushAsync(new PlantSettingsPage(plant, _plantRepository));
            }
        });
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
        var index = 1;

        foreach (var plant in plants)
        {
            plant.Index = index++;
            Plants.Add(plant);
            _ = Task.Run(async () => plant.SetImagePath(await _plantRepository.GetImagePathAsync(plant.Id)));
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