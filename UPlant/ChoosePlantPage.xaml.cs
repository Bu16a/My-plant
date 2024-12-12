using System.Collections.ObjectModel;
using UPlant.Domain.Interfaces;

namespace UPlant;

public partial class ChoosePlantPage : ContentPage
{
    private readonly IPlantRepository _plantRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    public ObservableCollection<string> Items { get; set; } = new();
    private readonly FileResult _image;

    public ChoosePlantPage(FileResult image, IPlantRepository plantRepository, IServiceProvider serviceProvider, INavigationService navigationService)
    {
        _image = image;
        _plantRepository = plantRepository;
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        InitializeComponent();
        BindingContext = this;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            ResultListView.IsVisible = false;

            var items = await _plantRepository.GetPossiblePlantsAsync(_image, maxAttempts: 3);
            if (items == null || items.Count == 0)
            {
                await DisplayAlert("Ошибка", "Ни одно растение не найдено", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                foreach (var item in items)
                    Items.Add(item);
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Ошибка", "Не удалось распознать растение", "OK");
            await Navigation.PopAsync();
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ResultListView.IsVisible = true;
        }
    }

    private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is string selectedItem)
        {
            await _navigationService.NavigateToAsync<PlantInfoPage>(selectedItem, _image, _plantRepository, _serviceProvider, _navigationService);
            ((ListView)sender).SelectedItem = null;
        }
    }
}