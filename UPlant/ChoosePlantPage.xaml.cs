using System.Collections.ObjectModel;
using System.ComponentModel;
using UPlant.Domain.Interfaces;

namespace UPlant;

public partial class ChoosePlantPage : ContentPage
{
    private readonly IPlantRepository _plantRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    public ObservableCollection<PlantSearchResult> Items { get; set; } = new();
    private readonly FileResult _image;

    public ChoosePlantPage(FileResult image, IPlantRepository plantRepository, IServiceProvider serviceProvider, INavigationService navigationService)
    {
        _image = image;
        _plantRepository = plantRepository;
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
        InitializeComponent();
        ResultCollectionView.ItemsSource = Items;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            ResultCollectionView.IsVisible = false;

            var items = await _plantRepository.GetPossiblePlantsAsync(_image, maxAttempts: 3);
            if (items == null || items.Count == 0)
            {
                await DisplayAlert("Ошибка", "Ни одно растение не найдено", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                foreach (var item in items)
                    Items.Add(new() { Text = item });
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
            ResultCollectionView.IsVisible = true;
        }
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PlantSearchResult searchResult)
        {
            await _navigationService.NavigateToAsync<PlantInfoPage>(searchResult.Text, _image, _plantRepository, _serviceProvider, _navigationService);
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}

public class PlantSearchResult
{
    public string Text { get; set; }
    public string ImageSource => "https://i.pinimg.com/originals/85/ca/90/85ca90f6723dd5327642e99b807a0cb6.jpg";
}