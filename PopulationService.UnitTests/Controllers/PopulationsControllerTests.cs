using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PopulationService.Controllers;
using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.UnitTests.Controllers;


public class PopulationsControllerTests
{
    private readonly Mock<ILogger<PopulationsController>> _loggerMock = new();
    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with a valid aggregation service.
    /// Input: A valid mock instance of IPopulationAggregationService.
    /// Expected: Constructor completes successfully without throwing an exception.
    /// </summary>
    [Fact]
    public void WhenValidAggregationService_ShouldCreateInstance()
    {
        // Arrange
        var mockAggregationService = new Mock<IPopulationAggregationService>();

        // Act
        var controller = new PopulationsController(mockAggregationService.Object, _loggerMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    /// <summary>
    /// Tests the constructor's behavior when provided with a null aggregation service.
    /// Input: null for the aggregationService parameter.
    /// Expected: Constructor completes without throwing an exception (no null guard in implementation).
    /// </summary>
    [Fact]
    public void WhenNullAggregationService_ShouldNotThrow()
    {
        // Arrange
        IPopulationAggregationService? nullService = null;

        // Act
        var controller = new PopulationsController(nullService!, _loggerMock.Object);

        // Assert
        Assert.NotNull(controller);
    }

    /// <summary>
    /// Tests that GetPopulations returns an OkObjectResult with the aggregated populations
    /// when the aggregation service returns a list of country populations.
    /// </summary>
    [Fact]
    public async Task GetPopulations_WhenServiceReturnsData_ShouldReturnOkResultWithData()
    {
        // Arrange
        var mockAggregationService = new Mock<IPopulationAggregationService>();

        var expectedPopulations = new List<CountryPopulation>
        {
            new("United States", 331000000),
            new("Canada", 38000000),
            new("Mexico", 128000000)
        };

        mockAggregationService.Setup(s => s.GetAggregatedPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPopulations);

        var controller = new PopulationsController(mockAggregationService.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetPopulations(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualPopulations = Assert.IsType<List<CountryPopulation>>(okResult.Value);
        Assert.Equal(3, actualPopulations.Count);
        Assert.Equal(expectedPopulations, actualPopulations);
    }

    /// <summary>
    /// Tests that GetPopulations returns an OkObjectResult with an empty list
    /// when the aggregation service returns an empty list.
    /// </summary>
    [Fact]
    public async Task GetPopulations_WhenServiceReturnsEmptyList_ShouldReturnOkResultWithEmptyList()
    {
        // Arrange
        var mockAggregationService = new Mock<IPopulationAggregationService>();

        var emptyList = new List<CountryPopulation>();

        mockAggregationService.Setup(s => s.GetAggregatedPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var controller = new PopulationsController(mockAggregationService.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetPopulations(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualPopulations = Assert.IsType<List<CountryPopulation>>(okResult.Value);
        Assert.Empty(actualPopulations);
    }

    /// <summary>
    /// Tests that GetPopulations returns an OkObjectResult with a single country
    /// when the aggregation service returns a list with one item.
    /// </summary>
    [Fact]
    public async Task GetPopulations_WhenServiceReturnsSingleItem_ShouldReturnOkResultWithSingleItem()
    {
        // Arrange
        var mockAggregationService = new Mock<IPopulationAggregationService>();

        var singleItemList = new List<CountryPopulation>
        {
            new("India", 1380000000)
        };

        mockAggregationService.Setup(s => s.GetAggregatedPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(singleItemList);

        var controller = new PopulationsController(mockAggregationService.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetPopulations(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualPopulations = Assert.IsType<List<CountryPopulation>>(okResult.Value);
        Assert.Single(actualPopulations);
        Assert.Equal("India", actualPopulations[0].CountryName);
        Assert.Equal(1380000000, actualPopulations[0].Population);
    }

    /// <summary>
    /// Tests that GetPopulations returns an OkObjectResult with null value
    /// when the aggregation service returns null (edge case for defensive testing).
    /// </summary>
    [Fact]
    public async Task GetPopulations_WhenServiceReturnsNull_ShouldReturnOkResultWithNull()
    {
        // Arrange
        var mockAggregationService = new Mock<IPopulationAggregationService>();

        mockAggregationService.Setup(s => s.GetAggregatedPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<CountryPopulation>?)null);

        var controller = new PopulationsController(mockAggregationService.Object, _loggerMock.Object);

        // Act
        var result = await controller.GetPopulations(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(okResult.Value);
    }
}