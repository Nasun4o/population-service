using PopulationService.Models;

namespace PopulationService.Interfaces
{
    public interface IPopulationAggregationService
    {
        Task<List<CountryPopulation>> GetAggregatedPopulationsAsync();
    }
}
