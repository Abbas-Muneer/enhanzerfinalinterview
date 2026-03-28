using Enhanzer.Api.Interfaces;
using Enhanzer.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController(ILocationsService locationService) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocations(CancellationToken cancellationToken)
    {
        var locations = await locationService.GetLocationsAsync(cancellationToken);
        return Ok(locations);
    }

}