namespace UPlant;

public partial class PlantInfoPage : ContentPage
{
    private readonly string genus;
    private int watering = 0;
    private FileResult image;

    public PlantInfoPage(string genus, FileResult image)
    {
        this.genus = genus;
        this.image = image;
        InitializeComponent();
        PlantNameLabel.Text = genus;
        LoadWateringFrequencyAsync();
    }

    private async void LoadWateringFrequencyAsync()
    {
        try
        {
            watering = await PlantDB.GetWateringHZ(genus);
            WateringFrequencyLabel.Text = $"Частота полива: раз в {watering} часа";
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
        if (watering == 0) return;
        var id = Guid.NewGuid().ToString();
        await PlantDB.SaveImage(image, id);
        PlantDB.AddPlant(new Plant { Genus = genus, Id = id, Name = genus, Watering = watering, NeedToNotify = false });
        await DisplayAlert("Успех", $"Добавлено в ваш список!", "OK");
        Application.Current.MainPage = new NavigationPage(new MyPlantsPage());
    }
}