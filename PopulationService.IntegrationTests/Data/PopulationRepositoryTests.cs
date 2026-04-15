using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PopulationService.Data;
using PopulationService.Data.Entities;

namespace PopulationService.IntegrationTests.Data;

public class PopulationRepositoryTests : IDisposable
{
    private readonly SqliteConnection _dbConnection;
    private readonly CountryPopulationDbContext _populationContext;
    private readonly CountryPopulationRepository _populationRepository;
    private readonly Mock<ILogger<CountryPopulationRepository>> _loggerMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PopulationRepositoryTests"/> class, setting up an in-memory SQLite database and configuring the test repository to use this database for integration testing.
    /// The constructor ensures that the database is created before any tests are run, allowing for consistent and isolated testing of the PopulationRepository's functionality.
    /// </summary>
    public PopulationRepositoryTests()
    {
        _dbConnection = new SqliteConnection("Data Source=:memory:");
        _dbConnection.Open();

        var options = new DbContextOptionsBuilder<CountryPopulationDbContext>()
            .UseSqlite(_dbConnection)
            .Options;

        _populationContext = new CountryPopulationDbContext(options);
        _populationContext.Database.EnsureCreated();

        _populationRepository = new CountryPopulationRepository(_populationContext, _loggerMock.Object);
    }

    /// <summary>City populations within a state are summed and attributed to their country.</summary>
    [Fact]
    public async Task GetCountryPopulations_WhenCitiesExistInState_ShouldAggregateCityPopulationsByCountry()
    {
        // Arrange
        SeedData(
            country: (id: 1, name: "TestCountry"),
            states: [(id: 1, name: "State1", countryId: 1)],
            cities: [(id: 1, name: "CityA", stateId: 1, population: 500000), (id: 2, name: "CityB", stateId: 1, population: 300000)]
        );

        // Act
        var result = await _populationRepository.GetCountryPopulationsAsync();

        // Assert
        var country = Assert.Single(result);
        Assert.Equal("TestCountry", country.CountryName);
        Assert.Equal(800000, country.Population);
    }

    /// <summary>Cities across multiple states belonging to the same country are all summed into one total.</summary>
    [Fact]
    public async Task GetCountryPopulations_WhenMultipleCitiesAcrossStates_ShouldSumPerCountry()
    {
        // Arrange
        SeedData(
            country: (id: 1, name: "USA"),
            states: [(id: 1, name: "California", countryId: 1), (id: 2, name: "Texas", countryId: 1)],
            cities: [(id: 1, name: "LA", stateId: 1, population: 4000000), (id: 2, name: "SF", stateId: 1, population: 800000), (id: 3, name: "Houston", stateId: 2, population: 2300000)]
        );

        // Act
        var result = await _populationRepository.GetCountryPopulationsAsync();

        // Assert
        var usa = Assert.Single(result);
        Assert.Equal(7100000, usa.Population);
    }

    /// <summary>A city with a null population is treated as zero and does not affect the country total.</summary>
    [Fact]
    public async Task GetCountryPopulations_WhenPopulationIsNull_ShouldTreatAsZero()
    {
        // Arrange
        SeedData(
            country: (id: 1, name: "TestCountry"),
            states: [(id: 1, name: "State1", countryId: 1)],
            cities: [(id: 1, name: "CityA", stateId: 1, population: 100000), (id: 2, name: "CityB", stateId: 1, population: null)]
        );

        // Act
        var result = await _populationRepository.GetCountryPopulationsAsync();

        // Assert
        var country = Assert.Single(result);
        Assert.Equal(100000, country.Population);
    }

    /// <summary>Each country is returned as a separate entry with its own aggregated population.</summary>
    [Fact]
    public async Task GetCountryPopulations_WhenMultipleCountriesExist_ShouldReturnSeparately()
    {
        // Arrange
        SeedCountry(1, "Canada");
        SeedCountry(2, "Mexico");
        SeedState(1, "Ontario", 1);
        SeedState(2, "Jalisco", 2);
        SeedCity(1, "Toronto", 1, 2700000);
        SeedCity(2, "Guadalajara", 2, 1500000);

        _populationContext.SaveChanges();

        // Act
        var result = await _populationRepository.GetCountryPopulationsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.CountryName == "Canada" && c.Population == 2700000);
        Assert.Contains(result, c => c.CountryName == "Mexico" && c.Population == 1500000);
    }

    /// <summary>An empty list is returned when no cities exist in the database.</summary>
    [Fact]
    public async Task GetCountryPopulations_WhenNoDataExists_ShouldReturnEmptyList()
    {
        // Act
        var result = await _populationRepository.GetCountryPopulationsAsync();

        // Assert
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _populationContext.Dispose();
        _dbConnection.Dispose();
    }

    private void SeedData(
        (int id, string name) country,
        (int id, string name, int countryId)[] states,
        (int id, string name, int stateId, int? population)[] cities)
    {
        SeedCountry(country.id, country.name);
        foreach (var state in states) SeedState(state.id, state.name, state.countryId);
        foreach (var city in cities) SeedCity(city.id, city.name, city.stateId, city.population);

        _populationContext.SaveChanges();
    }

    private void SeedCountry(int id, string name) =>
        _populationContext.Countries.Add(new CountryEntity { CountryId = id, CountryName = name });

    private void SeedState(int id, string name, int countryId) =>
        _populationContext.States.Add(new StateEntity { StateId = id, StateName = name, CountryId = countryId });

    private void SeedCity(int id, string name, int stateId, int? population) =>
        _populationContext.Cities.Add(new CityEntity { CityId = id, CityName = name, StateId = stateId, Population = population });
}
