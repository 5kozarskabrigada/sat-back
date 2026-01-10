using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SatMockPlatform.Api.Data;
using SatMockPlatform.Api.DTOs;
using SatMockPlatform.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SatMockPlatform.Api.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        
        Console.WriteLine($"[DEBUG] Login Attempt: User='{request.Username}' Found={(user != null)}");
        if (user != null)
        {
             var verify = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
             Console.WriteLine($"[DEBUG] Hash Verify: '{request.Password}' vs '{user.PasswordHash}' => {verify}");
        }

        // Note: In a real app, use a proper hashing mechanism validation.
        // Assuming BCrypt hash is stored.
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        return new LoginResponse(token, user.Role, user.Username);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Explicit mapping for controllers
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
