using System.Collections.ObjectModel;

namespace UPlant;
public partial class ChoosePlantPage : ContentPage
{
    public ObservableCollection<string> Items { get; set; } = new();
    private FileResult image;

    public ChoosePlantPage(FileResult image)
    {
        this.image = image;
        InitializeComponent();
        BindingContext = this;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        try
        {
            var items = await PlantDB.GetPossiblePlantsAsync(image);
            if (items == null || items.Count == 0)
            {
                await DisplayAlert("Ошибка", $"На фото растений не найдено", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                foreach (var item in items)
                    Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка распознавания. Попробуйте позже", "OK");
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
            await Navigation.PushAsync(new PlantInfoPage(selectedItem, image));
            ((ListView)sender).SelectedItem = null;
        }
    }
}