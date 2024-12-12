using NUnit.Framework;
using UPlant.Domain.Models;
using System.ComponentModel;

namespace UPlant.Tests.Domain;

[TestFixture]
public class PlantTests
{
    private Plant _plant;

    [SetUp]
    public void Setup()
    {
        _plant = new Plant
        {
            Id = "1",
            Name = "Test Plant",
            Genus = "TestGenus",
            Watering = 7,
            NeedToNotify = true
        };
    }

    [Test]
    public void UpdateName_ShouldChangePlantName()
    {
        // Act
        _plant.UpdateName("New Name");

        // Assert
        Assert.That(_plant.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public void UpdateWatering_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChanged = false;
        _plant.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(Plant.Watering))
                propertyChanged = true;
        };

        // Act
        _plant.UpdateWatering(14);

        // Assert
        Assert.That(propertyChanged, Is.True);
        Assert.That(_plant.Watering, Is.EqualTo(14));
    }

    [Test]
    public void SetImagePath_ShouldUpdateImageProperties()
    {
        // Act
        _plant.SetImagePath("test/path/image.jpg");

        // Assert
        Assert.That(_plant.ImagePath, Is.EqualTo("test/path/image.jpg"));
        Assert.That(_plant.IsImageLoading, Is.False);
        Assert.That(_plant.IsImageLoaded, Is.True);
    }
} 