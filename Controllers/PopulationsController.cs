using Microsoft.AspNetCore.Mvc;
using PopulationService.Interfaces;

namespace PopulationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PopulationsController : ControllerBase
{
    private readonly IPopulationAggregationService _aggregationService;

    public PopulationsController(IPopulationAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPopulations()
    {
        var populations = await _aggregationService.GetAggregatedPopulationsAsync();
        return Ok(populations);
    }
}
