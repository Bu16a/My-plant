using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UPlant
{
    public static class PlantDB
    {
        private static readonly string _server = "http://10.25.0.113:12345/";
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
            var response = await FirebaseApi.ReadDatabaseAsync("plants");
            if (response == "null")
                plants = new();
            else
                plants = JsonSerializer.Deserialize<List<Plant>>(response) ?? new();
        }

        public static async void SavePlantData() => await FirebaseApi.WriteDatabaseAsync("plants", plants);

        private static readonly HttpClient _httpClient = new();

        public static async Task<string> GetImagePath(string name)
        {
            var filePath = Path.Combine(_cache, name);
            if (File.Exists(filePath)) return filePath;
            var response = await _httpClient.GetAsync($"{_server}get-file&file={name}");
            await File.WriteAllBytesAsync(filePath, await response.Content.ReadAsByteArrayAsync());
            return filePath;
        }

        public static async Task<List<string>> GetPossiblePlantsAsync(Stream stream)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", Guid.NewGuid().ToString());
            var response = await _httpClient.PostAsync($"{_server}/upload", content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync()) ?? new();
        }
    }

    public class Plant
    {
        public Plant(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; private set; }
        public string Path { get; private set; }


        public void UpdatePath(string newPath)
        {
            Path = newPath;
            PlantDB.SavePlantData();
        }
        public void UpdateName(string newName)
        {
            Name = newName;
            PlantDB.SavePlantData();
        }
    }
}