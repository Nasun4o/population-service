using PopulationService.Models;

namespace PopulationService.Interfaces;

public interface ICountryPopulationRepository
{
    /// <summary>
    /// Retrieves the population of each country by aggregating the populations of its cities.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of countries and their populations.</returns>
    Task<List<CountryPopulation>> GetCountryPopulationsAsync(CancellationToken cancellationToken = default);
}
