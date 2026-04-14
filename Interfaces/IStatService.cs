using PopulationService.Models;

namespace PopulationService.Interfaces;

public interface IStatService
{
    Task<List<CountryPopulation>> GetCountryPopulationsAsync();
}
