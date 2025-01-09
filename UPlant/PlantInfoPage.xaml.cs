using System.Text.Json;
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

    public PlantInfoPage(string genus, string url, FileResult image, IPlantRepository plantRepository, IServiceProvider serviceProvider, INavigationService navigationService)
    {
        _genus = genus;
        _image = image;
        _plantRepository = plantRepository;
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        InitializeComponent();
        PlantNameLabel.Text = genus;
        PhotoImage.Source = url;
        LoadPlantInfoAsync();
    }

    private async void LoadPlantInfoAsync()
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            var plantInfo = await _plantRepository.GetPlantInfoAsync(_genus);

            if (plantInfo != null && plantInfo.Any())
            {
                if (plantInfo.TryGetValue("Частота полива (раз в неделю)", out var watering) && watering is JsonElement json && json.ValueKind == JsonValueKind.Number)
                    _watering = 168 / json.GetInt32();
                PlantInfoLabel.Text = string.Join("\n", plantInfo.Select(entry => $"{entry.Key}: {entry.Value}"));
                PlantInfoLabel.IsVisible = true;
            }
            else
            {
                await DisplayAlert("Информация", "Данные о растении отсутствуют.", "OK");
                await _navigationService.NavigateBackAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            await _navigationService.NavigateBackAsync();
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}