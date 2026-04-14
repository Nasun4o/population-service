using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.Services;

public class PopulationAggregationService : IPopulationAggregationService
{
    private readonly IPopulationRepository _repository;
    private readonly ICountryStatsService _countryStatsService;

    public PopulationAggregationService(
        IPopulationRepository repository,
        ICountryStatsService countryStatsService)
    {
        _repository = repository;
        _countryStatsService = countryStatsService;
    }

    public async Task<List<CountryPopulation>> GetAggregatedPopulationsAsync()
    {
        var countriesPopulationTaskDB = _repository.GetCountryPopulationsAsync();
        var countriesPopulationTaskExternal = _countryStatsService.GetCountryPopulationsAsync();

        await Task.WhenAll(countriesPopulationTaskDB, countriesPopulationTaskExternal);

        var countriesDataFromDB = countriesPopulationTaskDB.Result;
        var countriesDataFromExternalSource = countriesPopulationTaskExternal.Result;

        var mergedDataFromAllSources = new Dictionary<string, CountryPopulation>(StringComparer.OrdinalIgnoreCase);

        foreach (var country in countriesDataFromExternalSource)
        {
            mergedDataFromAllSources[country.CountryName] = country;
        }

        foreach (var country in countriesDataFromDB)
        {
            mergedDataFromAllSources[country.CountryName] = country;
        }

        return mergedDataFromAllSources.Values
            .OrderBy(c => c.CountryName)
            .ToList();
    }
}
