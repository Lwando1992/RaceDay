using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Storage;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _storage;

    public UsersController(ApplicationDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    /// <summary>View the logged-in user's profile.</summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> Profile()
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();
        return Ok(user.ToDto());
    }

    /// <summary>Update the logged-in user's profile.</summary>
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();

        user.FullName = request.FullName;
        user.ImageUrl = request.ImageUrl ?? user.ImageUrl;
        await _db.SaveChangesAsync();

        return Ok(user.ToDto());
    }

    /// <summary>Upload a profile picture (stored in Azure Blob Storage).</summary>
    [HttpPost("profile/image")]
    public async Task<ActionResult<UserDto>> UploadProfileImage(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) return BadRequest(new { message = "File is empty." });

        var user = await _db.Users.FindAsync(User.GetUserId());
        if (user is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(user.ImageUrl))
            await _storage.DeleteAsync(user.ImageUrl, ct);

        await using var stream = file.OpenReadStream();
        user.ImageUrl = await _storage.UploadAsync(stream, file.FileName, file.ContentType, ct);
        await _db.SaveChangesAsync();

        return Ok(user.ToDto());
    }
}
