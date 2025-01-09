using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace UPlant.Domain.Models;

public class Plant : INotifyPropertyChanged
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("genus")]
    public string Genus { get; set; }

    private int watering;
    [JsonPropertyName("watering")]
    public int Watering 
    { 
        get => watering;
        set
        {
            watering = value;
            OnPropertyChanged();
        }
    }

    [JsonPropertyName("is_notify")]
    public bool NeedToNotify { get; set; }

    private int index;
    [JsonIgnore]
    public int Index 
    { 
        get => index;
        set
        {
            index = value;
            OnPropertyChanged();
        }
    }

    private bool isImageLoading = true;
    private bool isImageLoaded = false;
    private string imagePath;

    [JsonIgnore]
    public bool IsImageLoading 
    { 
        get => isImageLoading; 
        private set 
        { 
            isImageLoading = value; 
            OnPropertyChanged(); 
        } 
    }

    [JsonIgnore]
    public bool IsImageLoaded 
    { 
        get => isImageLoaded; 
        private set 
        { 
            isImageLoaded = value; 
            OnPropertyChanged(); 
        } 
    }

    [JsonIgnore]
    public string ImagePath 
    { 
        get => imagePath; 
        private set 
        { 
            imagePath = value; 
            OnPropertyChanged(); 
        } 
    }

    public void UpdateName(string newName)
    {
        Name = newName;
    }

    public void UpdateWatering(int watering)
    {
        Watering = watering;
    }

    public void UpdateNotification(bool needToNotify)
    {
        NeedToNotify = needToNotify;
    }

    public void SetImagePath(string path)
    {
        ImagePath = path;
        IsImageLoading = false;
        IsImageLoaded = true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 