using Microsoft.EntityFrameworkCore;
using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.Data;

public class CountryPopulationRepository : ICountryPopulationRepository
{
    private readonly CountryPopulationDbContext _context;
    private readonly ILogger<CountryPopulationRepository> _logger;

    public CountryPopulationRepository(
        CountryPopulationDbContext context,
        ILogger<CountryPopulationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the population of each country by aggregating the populations of its cities.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of countries and their populations.</returns>
    public async Task<List<CountryPopulation>> GetCountryPopulationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Querying country populations from database.");

        var result = await _context.Cities
            .Include(c => c.State)
                .ThenInclude(s => s.Country)
            .GroupBy(c => c.State.Country.CountryName)
            .Select(g => new CountryPopulation(
                g.Key ?? "Unknown",
                g.Sum(c => c.Population ?? 0)))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} country records from database.", result.Count);

        return result;
    }
}
