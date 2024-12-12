using UPlant.Domain.Models;

namespace UPlant.Domain.Interfaces;

public interface IPlantRepository
{
    Task<IReadOnlyList<Plant>> GetAllPlantsAsync();
    Task AddPlantAsync(Plant plant);
    Task DeletePlantAsync(Plant plant);
    Task UpdatePlantAsync(Plant plant);
    Task<string> GetImagePathAsync(string id);
    Task SaveImageAsync(FileResult file, string name);
    Task<List<string>> GetPossiblePlantsAsync(FileResult file, int maxAttempts = 3);
    Task<int> GetWateringFrequencyAsync(string genus);
} 