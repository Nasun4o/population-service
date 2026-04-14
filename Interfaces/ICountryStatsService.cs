using PopulationService.Models;

namespace PopulationService.Interfaces;

public interface ICountryStatsService
{
    Task<List<CountryPopulation>> GetCountryPopulationsAsync();
}
