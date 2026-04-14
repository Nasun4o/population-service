using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.Services;

public class PopulationAggregationService : IPopulationAggregationService
{
    public Task<List<CountryPopulation>> GetAggregatedPopulationsAsync()
    {
        throw new NotImplementedException();
    }
}
