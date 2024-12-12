using NUnit.Framework;
using Moq;
using UPlant.Domain.Interfaces;
using UPlant.Infrastructure.Services;
using UPlant.Domain.Models;
using System.Text.Json;
using System.Net.Http;

namespace UPlant.Tests.Services;

[TestFixture]
public class PlantRepositoryTests
{
    private Mock<IFirebaseDatabase> _mockDatabase;
    private Mock<IAuthService> _mockAuth;
    private PlantRepository _repository;
    private List<Plant> _testPlants;
    private Mock<HttpClient> _mockHttpClient;

    [SetUp]
    public void Setup()
    {
        _mockDatabase = new Mock<IFirebaseDatabase>();
        _mockAuth = new Mock<IAuthService>();
        _testPlants = new List<Plant>
        {
            new Plant { Id = "1", Name = "Test Plant 1", Genus = "TestGenus1" },
            new Plant { Id = "2", Name = "Test Plant 2", Genus = "TestGenus2" }
        };

        _mockDatabase.Setup(x => x.ReadServerURLAsync())
            .ReturnsAsync("http://test-server.com/");
        
        _mockDatabase.Setup(x => x.ReadDatabaseAsync("plants"))
            .ReturnsAsync(JsonSerializer.Serialize(_testPlants));

        _mockAuth.Setup(x => x.IsAuthenticated).Returns(true);

        _mockHttpClient = new Mock<HttpClient>();

        _repository = new PlantRepository(_mockDatabase.Object, _mockAuth.Object, _mockHttpClient.Object);
    }

    [Test]
    public async Task GetAllPlants_ShouldReturnAllPlants()
    {
        // Act
        var plants = await _repository.GetAllPlantsAsync();

        // Assert
        Assert.That(plants, Has.Count.EqualTo(2));
        Assert.That(plants.First().Name, Is.EqualTo("Test Plant 1"));
    }

    [Test]
    public async Task AddPlant_ShouldAddPlantToList()
    {
        // Arrange
        var newPlant = new Plant { Id = "3", Name = "New Plant", Genus = "NewGenus" };

        // Act
        await _repository.AddPlantAsync(newPlant);
        var plants = await _repository.GetAllPlantsAsync();

        // Assert
        Assert.That(plants, Has.Count.EqualTo(3));
        _mockDatabase.Verify(x => x.WriteDatabaseAsync("plants", It.IsAny<List<Plant>>()), Times.Once);
    }

    [Test]
    public async Task DeletePlant_ShouldRemovePlantFromList()
    {
        // Act
        await _repository.DeletePlantAsync(_testPlants[0]);
        var plants = await _repository.GetAllPlantsAsync();

        // Assert
        Assert.That(plants, Has.Count.EqualTo(1));
        Assert.That(plants.First().Id, Is.EqualTo("2"));
        _mockDatabase.Verify(x => x.WriteDatabaseAsync("plants", It.IsAny<List<Plant>>()), Times.Once);
    }

    [Test]
    public async Task GetPossiblePlants_ShouldRetryOnFailure()
    {
        // Arrange
        var file = new Mock<FileResult>().Object;
        var expectedPlants = new List<string> { "Plant1", "Plant2" };
        var attemptCount = 0;

        _mockHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount < 2) // Первая попытка неудачная
                    throw new HttpRequestException("Test error");

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(expectedPlants));
                return response;
            });

        // Act
        var result = await _repository.GetPossiblePlantsAsync(file, maxAttempts: 3);

        // Assert
        Assert.That(result, Is.EqualTo(expectedPlants));
        Assert.That(attemptCount, Is.EqualTo(2));
    }

    [Test]
    public void GetPossiblePlants_ShouldThrowAfterMaxAttempts()
    {
        // Arrange
        var file = new Mock<FileResult>().Object;
        _mockHttpClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ThrowsAsync(new HttpRequestException("Test error"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => 
            await _repository.GetPossiblePlantsAsync(file, maxAttempts: 3));
        Assert.That(ex.Message, Does.Contain("3 попыток"));
    }
} 