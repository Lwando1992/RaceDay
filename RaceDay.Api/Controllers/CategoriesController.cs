using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db) => _db = db;

    /// <summary>Retrieve a list of all categories.</summary>
    [HttpGet("api/categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories() =>
        Ok((await _db.Categories.OrderBy(c => c.Name).ToListAsync()).Select(c => c.ToDto()));

    /// <summary>Create a new category for an event (Organiser only).</summary>
    [HttpPost("api/events/{eventId:int}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<CategoryDto>> Create(int eventId, [FromBody] CreateCategoryRequest request)
    {
        var @event = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.Id == eventId);
        if (@event is null) return NotFound(new { message = $"Event {eventId} not found." });
        if (@event.OrganiserId != User.GetUserId()) return Forbid();

        var category = new Category
        {
            EventId = eventId,
            Name = request.Name,
            Description = request.Description
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { eventId }, category.ToDto());
    }

    /// <summary>Update a category (Organiser only, own events).</summary>
    [HttpPut("api/categories/{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int categoryId, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category is null) return NotFound();
        if (category.Event.OrganiserId != User.GetUserId()) return Forbid();

        category.Name = request.Name;
        category.Description = request.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Delete a category (Organiser only, own events).</summary>
    [HttpDelete("api/categories/{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int categoryId)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category is null) return NotFound();
        if (category.Event.OrganiserId != User.GetUserId()) return Forbid();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
