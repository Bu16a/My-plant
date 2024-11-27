using Microsoft.Maui.Controls;
using System;
using System.IO;

namespace UPlant
{
    public partial class PlantSettingsPage : ContentPage
    {
        private readonly Plant plant;

        public PlantSettingsPage(Plant plant)
        {
            InitializeComponent();
            this.plant = plant;
            BindingContext = this.plant;
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            string newPlantName = await DisplayPromptAsync("", "", "Ок", "Отмена", "Новое имя", maxLength: 100, keyboard: Keyboard.Text);
            //await Socket.SendGetRequestAsync();
            if (!string.IsNullOrEmpty(newPlantName))
            {
                if (newPlantName != plant.Name)
                {
                    plant.UpdateName(newPlantName);
                    PlantDB.SavePlantData();
                    PhotoTitleLabel.Text = plant.Name;
                }
                await DisplayAlert("Успешно", "Успешно", "OK");
            }
        }

        private async void OnUpdatePhotoClicked(object sender, EventArgs e)
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                var newphotoFilePath = Path.Combine(Path.GetDirectoryName(plant.Path), $"{Guid.NewGuid()}.jpg");
                if (File.Exists(plant.Path))
                    File.Delete(plant.Path);
                using var stream = await photo.OpenReadAsync();
                using var fileStream = File.OpenWrite(newphotoFilePath);
                await stream.CopyToAsync(fileStream);
                plant.UpdatePath(newphotoFilePath);
                await DisplayAlert("Фото обновлено", "Фото обновлено", "OK");
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            if (await DisplayActionSheet("Вы уверены?", "Нет", "Да") == "Да")
            {
                PlantDB.DeletePlant(plant);
                await Navigation.PopAsync();
            }
        }
    }
}