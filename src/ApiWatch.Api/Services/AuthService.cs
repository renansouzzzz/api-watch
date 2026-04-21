using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiWatch.Core.Data;
using ApiWatch.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiWatch.Api.Services;

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Name, string Email, string PlanName);

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        // Reject duplicate emails
        if (await _db.Users.AnyAsync(u => u.Email == req.Email, ct))
            return null;

        var user = new User
        {
            Name = req.Name,
            Email = req.Email.ToLowerInvariant(),
            // BCrypt hashes the password with a random salt — we never store the plain text
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            PlanId = 1 // Free plan by default
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(user).Reference(u => u.Plan).LoadAsync(ct);

        return new AuthResponse(GenerateToken(user), user.Name, user.Email, user.Plan.Name);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant(), ct);

        if (user is null)
            return null;

        // BCrypt.Verify compares the plain password against the stored hash
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return null;

        return new AuthResponse(GenerateToken(user), user.Name, user.Email, user.Plan.Name);
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims are the data embedded inside the token (readable by anyone, but not forgeable)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("planId", user.PlanId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:ExpirationDays"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
