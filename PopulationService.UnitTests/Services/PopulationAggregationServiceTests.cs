using Microsoft.Extensions.Logging;
using Moq;
using PopulationService.Interfaces;
using PopulationService.Services;

namespace PopulationService.UnitTests.Services;

public class PopulationAggregationServiceTests
{
    private readonly Mock<ICountryPopulationRepository> _repositoryMock = new();
    private readonly Mock<ICountryStatsService> _countryStatsServiceMock = new();
    private readonly Mock<ILogger<PopulationAggregationService>> _loggerMock = new();
    private readonly PopulationAggregationService _populationAggregationService;

    public PopulationAggregationServiceTests()
    {
        _populationAggregationService = new PopulationAggregationService(
            _repositoryMock.Object,
            _countryStatsServiceMock.Object,
            _loggerMock.Object);
    }

    /// <summary>DB value takes precedence when the same country name exists in both sources.</summary>
    [Fact]
    public async Task GetAggregatedPopulations_WhenDuplicateCountryName_ShouldUseDbValueAsync()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("India", 1210000000)]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("India", 1182000000)]);

        // Act
        var result = await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1210000000, result[0].Population);
    }

    /// <summary>Country name matching is case-insensitive; DB value still wins on duplicate.</summary>
    [Fact]
    public async Task GetAggregatedPopulations_WhenDuplicateCountryNameWithDifferentCase_ShouldUseDbValue()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("india", 1210000000)]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("INDIA", 1182000000)]);

        // Act
        var result = await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1210000000, result[0].Population);
    }

    /// <summary>All countries from both sources are returned when there are no duplicate names.</summary>
    [Fact]
    public async Task GetAggregatedPopulations_WhenNoDuplicates_ShouldReturnUnionOfBothSources()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Canada", 37000000)]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Chile", 17000000)]);

        // Act
        var result = await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.CountryName == "Canada");
        Assert.Contains(result, c => c.CountryName == "Chile");
    }

    /// <summary>All countries from the non-empty source are returned when the other source is empty.</summary>
    [Fact]
    public async Task GetAggregatedPopulations_WhenOneSourceIsEmpty_ShouldReturnOtherSource()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Canada", 37000000), new("Egypt", 100000000)]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    /// <summary>An empty list is returned when both the DB and external source return no data.</summary>
    [Fact]
    public async Task GetAggregatedPopulations_WhenBothSourcesAreEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>The aggregated result is sorted alphabetically by country name.</summary>
    [Fact]
    public async Task GetAggregatedPopulations_WhenResultsReturned_ShouldBeSortedAlphabetically()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Zimbabwe", 15000000), new("Argentina", 45000000)]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Mali", 20000000)]);

        // Act
        var result = await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        Assert.Equal("Argentina", result[0].CountryName);
        Assert.Equal("Mali", result[1].CountryName);
        Assert.Equal("Zimbabwe", result[2].CountryName);
    }

    /// <summary> Verifies that the logger logs the expected message and that the log level is Information. </summary>
    [Fact]
    public async Task GetAggregatedPopulations_LogsExpectedMessage()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new("Canada", 37000000)]);

        _countryStatsServiceMock.Setup(s => s.GetCountryPopulationsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _populationAggregationService.GetAggregatedPopulationsAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Aggregation complete")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
