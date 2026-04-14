using Microsoft.EntityFrameworkCore;
using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.Data;

public class PopulationRepository : IPopulationRepository
{
    private readonly PopulationDbContext _context;

    public PopulationRepository(PopulationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryPopulation>> GetCountryPopulationsAsync()
    {
        return await _context.Cities
            .Include(c => c.State)
                .ThenInclude(s => s.Country)
            .GroupBy(c => c.State.Country.CountryName)
            .Select(g => new CountryPopulation(
                g.Key ?? "Unknown",
                g.Sum(c => c.Population ?? 0)))
            .ToListAsync();
    }
}
