namespace UPlant;

public partial class ChoosePlantPage : ContentPage
{
    public ChoosePlantPage(List<string> names)
    {
        InitializeComponent();
        StringsCollectionView.ItemsSource = names;
    }

    private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        await DisplayAlert("AAAA", "525252525252", "OK");
        //PlantDB.AddPlant(new Plant(e.CurrentSelection[0] as string, ))
    }
}
