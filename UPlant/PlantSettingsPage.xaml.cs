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
            string newPlantName = await DisplayPromptAsync("Введите новое имя", "", "Ок", "Отмена", "Новое имя", maxLength: 100, keyboard: Keyboard.Text);
            if (!string.IsNullOrEmpty(newPlantName))
            {
                if (newPlantName != plant.Name)
                {
                    plant.UpdateName(newPlantName);
                    PhotoTitleLabel.Text = plant.Name;
                }
                await DisplayAlert("Успешно!", "Имя изменено", "OK");
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

        private void OnNeedToNotifyToggled(object sender, ToggledEventArgs e)
        {
            plant.NeedToNotify = e.Value;
            PlantDB.SavePlantData();
        }

        private void OnWateringDragCompleted(object sender, EventArgs e)
        {
            PlantDB.SavePlantData();
        }
    }
}