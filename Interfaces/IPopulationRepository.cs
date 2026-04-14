using PopulationService.Models;

namespace PopulationService.Interfaces;

public interface IPopulationRepository
{
    Task<List<CountryPopulation>> GetCountryPopulationsAsync();
}
