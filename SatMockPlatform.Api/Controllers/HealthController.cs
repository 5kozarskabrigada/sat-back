using Microsoft.AspNetCore.Mvc;
using SatMockPlatform.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace SatMockPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> Get()
    {
        // "Deep Warm-up": Force EF Core initialization and open DB connection
        // This ensures the first real user query (Login) is fast.
        await _context.Database.CanConnectAsync();
        return Ok(new { status = "ok", db = "connected" });
    }
}
