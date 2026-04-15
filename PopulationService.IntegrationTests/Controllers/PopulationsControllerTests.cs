using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PopulationService.Data;
using PopulationService.Data.Entities;
using PopulationService.Models;

namespace PopulationService.IntegrationTests.Controllers;

public class PopulationsControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _dbConnection;
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PopulationsControllerTests"/> class, setting up an in-memory SQLite database and configuring the test web application factory to use this database for integration testing.
    /// The constructor ensures that the database is created and seeded with test data before any tests are run, allowing for consistent and isolated testing of the PopulationsController's functionality.
    /// </summary>
    /// <param name="factory">The web application factory used to create the test server and client.</param>
    public PopulationsControllerTests(WebApplicationFactory<Program> factory)
    {
        _dbConnection = new SqliteConnection("Data Source=:memory:");
        _dbConnection.Open();

        var options = new DbContextOptionsBuilder<CountryPopulationDbContext>()
            .UseSqlite(_dbConnection)
            .Options;

        using (var context = new CountryPopulationDbContext(options))
        {
            context.Database.EnsureCreated();
            SeedTestData(context);
        }

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var serviceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CountryPopulationDbContext>));

                if (serviceDescriptor != null)
                {
                    services.Remove(serviceDescriptor);
                }

                services.AddDbContext<CountryPopulationDbContext>(opt =>
                    opt.UseSqlite(_dbConnection));
            });
        });
    }

    /// <summary>The endpoint returns HTTP 200 OK for a valid GET request.</summary>
    [Fact]
    public async Task GetPopulations_WhenValidRequest_ShouldReturnSuccessStatusCode()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var response = await client.GetAsync("/api/populations");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>The endpoint returns a non-empty JSON array for a valid GET request.</summary>
    [Fact]
    public async Task GetPopulations_WhenValidRequest_ShouldReturnJsonArray()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var populations = await client.GetFromJsonAsync<List<CountryPopulation>>("/api/populations");

        //Assert
        Assert.NotNull(populations);
        Assert.True(populations.Count > 0);
    }

    /// <summary>The response includes all countries that were seeded into the database.</summary>
    [Fact]
    public async Task GetPopulations_WhenResponseContainsSeededDbCountries_ShouldReturnExpectedCountries()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var populations = await client.GetFromJsonAsync<List<CountryPopulation>>("/api/populations");

        //Assert
        Assert.NotNull(populations);
        Assert.Contains(populations, p => p.CountryName == "United States");
        Assert.Contains(populations, p => p.CountryName == "Germany");
    }

    /// <summary>City populations are correctly summed per country and returned in the response.</summary>
    [Fact]
    public async Task GetPopulations_WhenValidRequest_ShouldReturnAggregatedPopulationsCorrectly()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var populations = await client.GetFromJsonAsync<List<CountryPopulation>>("/api/populations");

        //Assert
        Assert.NotNull(populations);
        var us = populations.FirstOrDefault(p => p.CountryName == "United States");
        Assert.NotNull(us);
        Assert.Equal(7170000, us.Population);

        var de = populations.FirstOrDefault(p => p.CountryName == "Germany");
        Assert.NotNull(de);
        Assert.Equal(1500000, de.Population);
    }

    /// <summary>The response includes countries from the external stats service in addition to the seeded DB countries.</summary>
    [Fact]
    public async Task GetPopulations_WhenIncludesStatServiceCountries_ShouldReturnAdditionalCountries()
    {      
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var populations = await client.GetFromJsonAsync<List<CountryPopulation>>("/api/populations");

        //Assert
        Assert.NotNull(populations);
        Assert.True(populations.Count > 2, "Should include more than just the 2 seeded DB countries");
    }

    /// <summary>The response list is sorted alphabetically by country name.</summary>
    [Fact]
    public async Task GetPopulations_WhenValidRequest_ShouldReturnSortedAlphabetically()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var populations = await client.GetFromJsonAsync<List<CountryPopulation>>("/api/populations");

        //Assert
        Assert.NotNull(populations);
        var names = populations.Select(p => p.CountryName).ToList();
        Assert.Equal(names.OrderBy(n => n), names);
    }

    private static void SeedTestData(CountryPopulationDbContext context)
    {
        var usa = new CountryEntity { CountryId = 1, CountryName = "United States" };
        var germany = new CountryEntity { CountryId = 2, CountryName = "Germany" };

        context.Countries.AddRange(usa, germany);

        var california = new StateEntity { StateId = 1, StateName = "California", CountryId = 1 };
        var texas = new StateEntity { StateId = 2, StateName = "Texas", CountryId = 1 };
        var bavaria = new StateEntity { StateId = 3, StateName = "Bavaria", CountryId = 2 };

        context.States.AddRange(california, texas, bavaria);

        context.Cities.AddRange(
            new CityEntity { CityId = 1, CityName = "Los Angeles", Population = 4000000, StateId = 1 },
            new CityEntity { CityId = 2, CityName = "San Francisco", Population = 870000, StateId = 1 },
            new CityEntity { CityId = 3, CityName = "Houston", Population = 2300000, StateId = 2 },
            new CityEntity { CityId = 4, CityName = "Munich", Population = 1500000, StateId = 3 }
        );

        context.SaveChanges();
    }

    public void Dispose()
    {
        _dbConnection.Dispose();
    }
}
