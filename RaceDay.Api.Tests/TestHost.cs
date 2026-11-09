using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Storage;
using RaceDay.Api.Models;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Tests;

/// <summary>Shared helpers for building controllers against an in-memory database.</summary>
public static class TestHost
{
    public static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static IConfiguration Config() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-signing-key-0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "RaceDay.Api.Test",
                ["Jwt:Audience"] = "RaceDay.Client.Test",
                ["Jwt:ExpiryHours"] = "2"
            })
            .Build();

    public static AuthController AuthController(ApplicationDbContext db) =>
        new(db, Config());

    public static UsersController UsersController(ApplicationDbContext db) =>
        new(db, new FakeStorage());

    /// <summary>Sets the authenticated user on a controller (id + role claims).</summary>
    public static void SetUser(ControllerBase controller, int userId, string role)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Role, role)
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal(identity) }
        };
    }

    private sealed class FakeStorage : IFileStorageService
    {
        public Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
            => Task.FromResult($"memory://{fileName}");

        public Task DeleteAsync(string fileUrl, CancellationToken ct = default) => Task.CompletedTask;
    }
}

/// <summary>Convenience factory for domain objects.</summary>
public static class Factories
{
    public static User User(string name = "Test User", string email = "user@test.local", string role = "Participant") =>
        new() { FullName = name, Email = email, Role = role, PasswordHash = "pw".Hash() };

    public static Event Event(int organiserId, string name = "City 10K") =>
        new() { Name = name, Date = DateTime.UtcNow.AddDays(30), Location = "City Centre", OrganiserId = organiserId };

    public static Category Category(int eventId, string name = "Senior") =>
        new() { EventId = eventId, Name = name };

    public static string Hash(this string password) => RaceDay.Api.Extensions.MappingExtensions.HashPassword(password);
}
