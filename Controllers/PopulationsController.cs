using Microsoft.AspNetCore.Mvc;
using PopulationService.Interfaces;
using PopulationService.Models;

namespace PopulationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PopulationsController : ControllerBase
{
    private readonly IPopulationAggregationService _aggregationService;
    private readonly ILogger<PopulationsController> _logger;

    public PopulationsController(
        IPopulationAggregationService aggregationService,
        ILogger<PopulationsController> logger)
    {
        _aggregationService = aggregationService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the aggregated populations for all countries.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of countries and their populations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CountryPopulation>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPopulations(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received GET /api/populations request.");

        var populations = await _aggregationService.GetAggregatedPopulationsAsync(cancellationToken);

        _logger.LogInformation("Returning {Count} aggregated country populations.", populations?.Count ?? 0);

        return Ok(populations);
    }
}
