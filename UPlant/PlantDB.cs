using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UPlant;
public static class PlantDB
{
    private static string _server;
    private static readonly string _cache = FileSystem.CacheDirectory;
    private static List<Plant> plants;
    public static IReadOnlyList<Plant> Plants => plants;

    public static void AddPlant(Plant plant)
    {
        plants.Add(plant);
        SavePlantData();
    }

    public static void DeletePlant(Plant plant)
    {
        plants.Remove(plant);
        SavePlantData();
    }

    public async static void LoadPlantData()
    {
        _server = await FirebaseApi.ReadServerURL();
        var response = await FirebaseApi.ReadDatabaseAsync("plants");
        if (response == "null")
            plants = new();
        else
            plants = JsonSerializer.Deserialize<List<Plant>>(response) ?? new();
    }

    public static async void SavePlantData() => await FirebaseApi.WriteDatabaseAsync("plants", plants);

    private static readonly HttpClient _httpClient = new();

    public static async Task<string> GetImagePath(string id)
    {
        var filePath = Path.Combine(_cache, $"{id}.jpg");
        if (File.Exists(filePath)) return filePath;
        var content = new StringContent(JsonSerializer.Serialize(new Dictionary<string,string>() { { "identifier", id } }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_server}get-image-by-id", content);
        await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
        return filePath;
    }

    public static async Task SaveImage(FileResult fileResult)
    {

    }

    public static async Task<List<string>> GetPossiblePlantsAsync(FileResult file)
    {
        var content = new MultipartFormDataContent { { new StreamContent(await file.OpenReadAsync()), "file", file.FileName } };
        var response = await _httpClient.PostAsync($"{_server}image-analysis-file", content);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());
    }

    public static async Task<int> GetWateringHZ(string genus)
    {
        var content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string>() { { "flower", genus } }), Encoding.UTF8, "application/json");
        var result = await _httpClient.PostAsync($"{_server}get_gz", content);
        result.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<Dictionary<string, int>>(await result.Content.ReadAsStringAsync())["hz"];
    }
}

public class Plant(string name, string path, string genus, int watering)
{
    public string Name { get; private set; } = name;
    public string Path { get; set; } = path;
    public string Genus { get; } = genus;
    public int Watering { get; } = watering;
    public bool NeedToNotify { get; } = false;

    public void UpdateName(string newName)
    {
        Name = newName;
        PlantDB.SavePlantData();
    }

    //private string _imagePath;
    //public string ImagePath
    //{
    //    get => _imagePath;
    //    set
    //    {
    //        if (_imagePath != value)
    //        {
    //            _imagePath = value;
    //            OnPropertyChanged();
    //        }
    //    }
    //}
    //public async Task LoadImagePathAsync()
    //{
    //    ImagePath = await PlantDB.GetImagePath(Path);
    //}


    //public event PropertyChangedEventHandler PropertyChanged;

    //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}
}