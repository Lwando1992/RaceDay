using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>Register a new user (Organiser or Participant).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "Email is already registered." });

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role == "Organiser" ? "Organiser" : "Participant",
            PasswordHash = request.Password.HashPassword()
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created("", new AuthResponse
        {
            Token = user.GenerateJwt(_config),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(2),
            User = user.ToDto()
        });
    }

    /// <summary>Log in a registered user and receive a JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null || user.PasswordHash != request.Password.HashPassword())
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new AuthResponse
        {
            Token = user.GenerateJwt(_config),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(2),
            User = user.ToDto()
        });
    }

    /// <summary>
    /// Log out the current user. The API is stateless (JWT), so logout is performed
    /// client-side by discarding the token; this endpoint confirms the call.
    /// </summary>
    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout() => Ok(new { message = "Logged out. Discard the bearer token on the client." });
}
