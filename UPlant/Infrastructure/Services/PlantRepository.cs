using UPlant.Domain.Interfaces;
using UPlant.Domain.Models;
using System.Text.Json;
using System.Net.Http.Headers;

namespace UPlant.Infrastructure.Services;

public class PlantRepository : IPlantRepository
{
    private readonly IFirebaseDatabase _database;
    private readonly IAuthService _authService;
    private readonly HttpClient _httpClient;
    private readonly string _cacheDirectory;
    private string _serverUrl;
    private List<Plant> _plants;

    public PlantRepository(IFirebaseDatabase database, IAuthService authService, HttpClient httpClient = null)
    {
        _database = database;
        _authService = authService;
        _httpClient = httpClient ?? new HttpClient();
        _cacheDirectory = FileSystem.CacheDirectory;
        _plants = new List<Plant>();
        
        _authService.AuthStateChanged += OnAuthStateChanged;
        
        InitializeAsync().Wait();
    }

    private async void OnAuthStateChanged(object? sender, bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            await LoadPlantsAsync();
        }
        else
        {
            _plants.Clear();
        }
    }

    private async Task InitializeAsync()
    {
        _serverUrl = await _database.ReadServerURLAsync();
        if (_authService.IsAuthenticated)
        {
            await LoadPlantsAsync();
        }
    }

    private async Task LoadPlantsAsync()
    {
        var response = await _database.ReadDatabaseAsync("plants");
        if (response != "null")
        {
            _plants = JsonSerializer.Deserialize<List<Plant>>(response) ?? new List<Plant>();
        }
    }

    public async Task<IReadOnlyList<Plant>> GetAllPlantsAsync()
    {
        return _plants.AsReadOnly();
    }

    public async Task AddPlantAsync(Plant plant)
    {
        _plants.Add(plant);
        await SavePlantsAsync();
    }

    public async Task DeletePlantAsync(Plant plant)
    {
        _plants.Remove(plant);
        await SavePlantsAsync();
    }

    public async Task UpdatePlantAsync(Plant plant)
    {
        await SavePlantsAsync();
    }

    private async Task SavePlantsAsync()
    {
        await _database.WriteDatabaseAsync("plants", _plants);
    }

    public async Task<string> GetImagePathAsync(string id)
    {
        var filePath = Path.Combine(_cacheDirectory, $"{id}.jpg");
        if (File.Exists(filePath)) return filePath;

        var content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string>() { { "identifier", id } }));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await _httpClient.PostAsync($"{_serverUrl}get-image-by-id", content);
        await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
        return filePath;
    }

    public async Task SaveImageAsync(FileResult file, string name)
    {
        using var multipartContent = new MultipartFormDataContent();
        var fileContent = new StreamContent(await file.OpenReadAsync());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        multipartContent.Add(fileContent, "file", name);
        multipartContent.Add(new StringContent(name), "identifier");
        
        var response = await _httpClient.PostAsync($"{_serverUrl}save-image-with-id", multipartContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<string>> GetPossiblePlantsAsync(FileResult file, int maxAttempts = 3)
    {
        Exception lastException = null;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var content = new MultipartFormDataContent
                {
                    { new StreamContent(await file.OpenReadAsync()), "file", file.FileName }
                };
                var response = await _httpClient.PostAsync($"{_serverUrl}image-analysis-file", content);
                response.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    await Task.Delay(1000 * attempt);
                    continue;
                }
            }
        }
        throw new Exception($"Не удалось распознать растение после {maxAttempts} попыток", lastException);
    }

    public async Task<int> GetWateringFrequencyAsync(string genus)
    {
        var content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string>() { { "flower", genus } }));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var result = await _httpClient.PostAsync($"{_serverUrl}get_gz", content);
        result.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<Dictionary<string, int>>(await result.Content.ReadAsStringAsync())["hz"];
    }

    public void Dispose()
    {
        _authService.AuthStateChanged -= OnAuthStateChanged;
    }
} 