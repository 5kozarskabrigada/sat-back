using Microsoft.AspNetCore.Mvc;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Services;

namespace SatMockPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized("Invalid credentials");
        }
        return Ok(result);
    }
}
