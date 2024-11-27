using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace UPlant
{
    public partial class MyPlantsPage : ContentPage
    {
        public MyPlantsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            PhotoCollectionView.ItemsSource = null;
            PhotoCollectionView.ItemsSource = PlantDB.Plants;
        }

        private async void OnPhotoSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                await Navigation.PushAsync(new PlantSettingsPage(e.CurrentSelection[0] as Plant));
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void OnMyPlantsClicked(object sender, EventArgs e) { }

        private async void OnPhotosClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}
