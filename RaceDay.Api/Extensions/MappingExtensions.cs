using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Models;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Extensions;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        ImageUrl = user.ImageUrl
    };

    public static CategoryDto ToDto(this Category category) => new()
    {
        Id = category.Id,
        EventId = category.EventId,
        Name = category.Name,
        Description = category.Description
    };

    public static EventDto ToDto(this Event @event) => new()
    {
        Id = @event.Id,
        Name = @event.Name,
        Description = @event.Description,
        Date = @event.Date,
        Location = @event.Location,
        OrganiserId = @event.OrganiserId,
        OrganiserName = @event.Organiser?.FullName,
        Categories = @event.Categories.Select(c => c.ToDto()).ToList()
    };

    public static EnrolmentDto ToDto(this Enrolment enrolment) => new()
    {
        Id = enrolment.Id,
        EventId = enrolment.EventId,
        EventName = enrolment.Event?.Name ?? string.Empty,
        CategoryId = enrolment.CategoryId,
        CategoryName = enrolment.Category?.Name ?? string.Empty,
        UserId = enrolment.UserId,
        UserName = enrolment.User?.FullName ?? string.Empty,
        EnrolledAtUtc = enrolment.EnrolledAtUtc
    };

    public static ResultDto ToDto(this Result result) => new()
    {
        Id = result.Id,
        EnrolmentId = result.EnrolmentId,
        EventId = result.Enrolment.EventId,
        EventName = result.Enrolment.Event?.Name ?? string.Empty,
        CategoryName = result.Enrolment.Category?.Name ?? string.Empty,
        UserId = result.Enrolment.UserId,
        UserName = result.Enrolment.User?.FullName ?? string.Empty,
        FinishTimeSeconds = result.FinishTimeSeconds,
        Position = result.Position,
        RecordedAtUtc = result.RecordedAtUtc
    };

    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("No user id claim present."));

    public static string HashPassword(this string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password + "|raceday-salt");
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public static string GenerateJwt(this User user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(double.TryParse(config["Jwt:ExpiryHours"], out var h) ? h : 2);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
