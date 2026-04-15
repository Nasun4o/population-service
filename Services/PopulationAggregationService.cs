using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.Services;

/// <summary>
/// Provides functionality to aggregate and retrieve country population data from multiple sources, ensuring that the
/// most recent and accurate information is returned to consumers.
/// </summary>
/// <remarks>This service combines population data from both a local database and an external source. When
/// conflicts occur, data from the external source takes precedence. The service is intended for scenarios where
/// up-to-date and comprehensive country population statistics are required. Thread safety and performance
/// considerations depend on the underlying repository and external service implementations.</remarks>
public class PopulationAggregationService : IPopulationAggregationService
{
    private readonly ICountryPopulationRepository _repository;
    private readonly ICountryStatsService _countryStatsService;
    private readonly ILogger<PopulationAggregationService> _logger;

    public PopulationAggregationService(
        ICountryPopulationRepository repository,
        ICountryStatsService countryStatsService,
        ILogger<PopulationAggregationService> logger)
    {
        _repository = repository;
        _countryStatsService = countryStatsService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves and aggregates population data from both the database and an external source, ensuring that the most recent data is used. 
    /// The method merges the data based on country names, giving precedence to the external source in case of conflicts, and returns a sorted list of country populations.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of countries and their populations.</returns>
    public async Task<List<CountryPopulation>> GetAggregatedPopulationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting population aggregation from DB and external source.");

        var countriesPopulationTaskDB = _repository.GetCountryPopulationsAsync(cancellationToken);
        var countriesPopulationTaskExternal = _countryStatsService.GetCountryPopulationsAsync(cancellationToken);

        await Task.WhenAll(countriesPopulationTaskDB, countriesPopulationTaskExternal);

        var countriesDataFromDB = await countriesPopulationTaskDB;
        var countriesDataFromExternalSource = await countriesPopulationTaskExternal;

        _logger.LogInformation("Fetched {DbCount} records from DB and {ExternalCount} from external source.", countriesDataFromDB.Count, countriesDataFromExternalSource.Count);

        var mergedDataFromAllSources = new Dictionary<string, CountryPopulation>(StringComparer.OrdinalIgnoreCase);

        foreach (var country in countriesDataFromExternalSource)
        {
            mergedDataFromAllSources[country.CountryName] = country;
        }

        foreach (var country in countriesDataFromDB)
        {
            mergedDataFromAllSources[country.CountryName] = country;
        }

        var result = mergedDataFromAllSources.Values
            .OrderBy(c => c.CountryName)
            .ToList();

        _logger.LogInformation("Aggregation complete. Returning {TotalCount} countries.", result.Count);

        return result;
    }
}
