using Microsoft.Maui.Controls;
using System;
using System.IO;
using UPlant.Domain.Interfaces;
using UPlant.Domain.Models;

namespace UPlant;

public partial class PlantSettingsPage : ContentPage
{
    private readonly Plant _plant;
    private readonly IPlantRepository _plantRepository;
    private bool _isUpdating = false;

    public PlantSettingsPage(Plant plant, IPlantRepository plantRepository)
    {
        InitializeComponent();
        _plant = plant;
        _plantRepository = plantRepository;
        BindingContext = _plant;
    }

    private async void OnSaveButtonClicked(object sender, EventArgs e)
    {
        string newPlantName = await DisplayPromptAsync("Введите новое имя", "", "Ок", "Отмена", "Новое имя", maxLength: 100, keyboard: Keyboard.Text);
        if (!string.IsNullOrEmpty(newPlantName))
        {
            if (newPlantName != _plant.Name)
            {
                _plant.UpdateName(newPlantName);
                await _plantRepository.UpdatePlantAsync(_plant);
                PhotoTitleLabel.Text = _plant.Name;
            }
            await DisplayAlert("Готово!", "Имя изменено", "OK");
        }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (await DisplayActionSheet("Вы уверены?", "Нет", "Да") == "Да")
        {
            await _plantRepository.DeletePlantAsync(_plant);
            await Navigation.PopAsync();
        }
    }

    private async void OnNeedToNotifyToggled(object sender, ToggledEventArgs e)
    {
        _plant.UpdateNotification(e.Value);
        await _plantRepository.UpdatePlantAsync(_plant);
    }

    private async void OnWateringValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        try
        {
            var roundedValue = (int)Math.Round(e.NewValue);
            _plant.UpdateWatering(roundedValue);
        }
        finally
        {
            await Task.Delay(50);
            _isUpdating = false;
        }
    }

    private async void OnWateringDragCompleted(object sender, EventArgs e)
    {
        await _plantRepository.UpdatePlantAsync(_plant);
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}