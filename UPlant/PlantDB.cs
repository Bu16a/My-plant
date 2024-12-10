using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


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
        var content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string>() { { "identifier", id } }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_server}get-image-by-id", content);
        await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
        return filePath;
    }

    public static async Task SaveImage(FileResult file, string name)
    {
        using var multipartContent = new MultipartFormDataContent();
        var fileContent = new StreamContent(await file.OpenReadAsync());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        multipartContent.Add(fileContent, "file", name);
        multipartContent.Add(new StringContent(name), "identifier");
        var response = await _httpClient.PostAsync($"{_server}save-image-with-id", multipartContent);
        response.EnsureSuccessStatusCode();
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

public class Plant : INotifyPropertyChanged
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("genus")]
    public string Genus { get; set; }

    [JsonIgnore]
    private int watering;
    [JsonPropertyName("watering")]
    public int Watering { get => watering; set { watering = value; OnPropertyChanged(); } }

    [JsonPropertyName("is_notify")]
    public bool NeedToNotify { get; set; }


    [JsonIgnore]
    private bool isImageLoading = true;
    [JsonIgnore]
    private bool isImageLoaded = false;
    [JsonIgnore]
    private string imagePath;

    [JsonIgnore]
    public bool IsImageLoading { get => isImageLoading; set { isImageLoading = value; OnPropertyChanged(); } }

    [JsonIgnore]
    public bool IsImageLoaded { get => isImageLoaded; set { isImageLoaded = value; OnPropertyChanged(); } }

    [JsonIgnore]
    public string ImagePath { get => imagePath; private set { imagePath = value; OnPropertyChanged(); } }

    public async Task LoadImagePathAsync()
    {
        IsImageLoading = true;
        ImagePath = await PlantDB.GetImagePath(Id);
        IsImageLoading = false;
        IsImageLoaded = true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        PlantDB.SavePlantData();
    }
}