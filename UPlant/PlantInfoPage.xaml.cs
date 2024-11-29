namespace UPlant;

public partial class PlantInfoPage : ContentPage
{
    private readonly string genus;
    private int watering = 0;

    public PlantInfoPage(string genus)
    {
        InitializeComponent();
        this.genus = genus;
        PlantNameLabel.Text = genus;
        LoadWateringFrequencyAsync();
    }

    private async void LoadWateringFrequencyAsync()
    {
        try
        {
            watering = await PlantDB.GetWateringHZ(genus);
            WateringFrequencyLabel.Text = $"������� ������: ��� � {watering} ����";
            WateringFrequencyLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� ��������� ������: {ex.Message}", "OK");
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

        PlantDB.AddPlant(new Plant(genus, "123", genus, watering));
        await DisplayAlert("�����", $"��������� � ��� ������!", "OK");
        Application.Current.MainPage = new NavigationPage(new MyPlantsPage());
    }
}