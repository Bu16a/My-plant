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
                {
                    var res = new PlantSearchResult() { Text = item, IsImageLoading = true };
                    Items.Add(res);
                    _ = Task.Run(async () => await res.FindImageURLAsync(_plantRepository));
                }
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

public class PlantSearchResult : INotifyPropertyChanged
{
    private string _imageSource;
    private bool _isImageLoading;

    public string Text { get; set; }

    public string ImageSource
    {
        get => _imageSource;
        set
        {
            _imageSource = value;
            OnPropertyChanged(nameof(ImageSource));
        }
    }

    public bool IsImageLoading
    {
        get => _isImageLoading;
        set
        {
            _isImageLoading = value;
            OnPropertyChanged(nameof(IsImageLoading));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task FindImageURLAsync(IPlantRepository plantRepository)
    {
        ImageSource = await plantRepository.GetGooglePlantImage(Text);
        IsImageLoading = false;
    }
}