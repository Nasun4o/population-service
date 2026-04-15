using PopulationService.Models;

namespace PopulationService.Interfaces
{
    public interface IPopulationAggregationService
    {
        /// <summary>
        /// Retrieves the aggregated population data for all countries.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A list of countries and their populations.</returns>
        Task<List<CountryPopulation>> GetAggregatedPopulationsAsync(CancellationToken cancellationToken = default);
    }
}
