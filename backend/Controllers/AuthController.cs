using Enhanzer.Api.Interfaces;
using Enhanzer.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
     public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await authService.LoginAsync(request, cancellationToken);
        if (!response.Success)
        {
            return Unauthorized(new ErrorResponseDto { Message = response.Message });
        }

        return Ok(response);
    }



}