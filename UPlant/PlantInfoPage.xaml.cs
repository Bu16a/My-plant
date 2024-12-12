using UPlant.Domain.Interfaces;
using UPlant.Domain.Models;

namespace UPlant;

public partial class PlantInfoPage : ContentPage
{
    private readonly string _genus;
    private int _watering = 0;
    private readonly FileResult _image;
    private readonly IPlantRepository _plantRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;

    public PlantInfoPage(string genus, FileResult image, IPlantRepository plantRepository, IServiceProvider serviceProvider, INavigationService navigationService)
    {
        _genus = genus;
        _image = image;
        _plantRepository = plantRepository;
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        InitializeComponent();
        PlantNameLabel.Text = genus;
        LoadWateringFrequencyAsync();
    }

    private async void LoadWateringFrequencyAsync()
    {
        try
        {
            _watering = await _plantRepository.GetWateringFrequencyAsync(_genus);
            WateringFrequencyLabel.Text = $"Частота полива: раз в {_watering} часов";
            WateringFrequencyLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            await Navigation.PopAsync();
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddPlantClicked(object sender, EventArgs e)
    {
        try
        {
            if (_watering == 0) return;
            AddButton.IsEnabled = false;
            var id = Guid.NewGuid().ToString();
            await _plantRepository.SaveImageAsync(_image, id);
            
            var plant = new Plant 
            { 
                Genus = _genus, 
                Id = id, 
                Name = _genus, 
                Watering = _watering, 
                NeedToNotify = false 
            };
            
            await _plantRepository.AddPlantAsync(plant);
            await DisplayAlert("Успех", "Добавлено в ваш список!", "OK");

            await _navigationService.SetMainPageAsync<MyPlantsPage>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось добавить растение: {ex.Message}", "OK");
        }
        finally
        {
            AddButton.IsEnabled = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }
}