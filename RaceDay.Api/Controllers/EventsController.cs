using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Api.Models;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventsController(ApplicationDbContext db) => _db = db;

    /// <summary>Retrieve a list of all events.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents() =>
        Ok((await _db.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .OrderBy(e => e.Date)
                .ToListAsync())
            .Select(e => e.ToDto()));

    /// <summary>Retrieve details of a specific event.</summary>
    [HttpGet("{eventId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDto>> GetEvent(int eventId)
    {
        var @event = await _db.Events
            .Include(e => e.Organiser)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        return @event is null ? NotFound() : Ok(@event.ToDto());
    }

    /// <summary>Create a new event (Organiser only).</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventRequest request)
    {
        var @event = new Event
        {
            Name = request.Name,
            Description = request.Description,
            Date = request.Date,
            Location = request.Location,
            OrganiserId = User.GetUserId()
        };

        _db.Events.Add(@event);
        await _db.SaveChangesAsync();

        @event = await _db.Events.Include(e => e.Organiser).FirstAsync(e => e.Id == @event.Id);
        return CreatedAtAction(nameof(GetEvent), new { eventId = @event.Id }, @event.ToDto());
    }

    /// <summary>Update an existing event (Organiser only, own events).</summary>
    [HttpPut("{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int eventId, [FromBody] UpdateEventRequest request)
    {
        var @event = await _db.Events.FindAsync(eventId);
        if (@event is null) return NotFound();
        if (@event.OrganiserId != User.GetUserId()) return Forbid();

        @event.Name = request.Name;
        @event.Description = request.Description;
        @event.Date = request.Date;
        @event.Location = request.Location;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Delete an event (Organiser only, own events).</summary>
    [HttpDelete("{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int eventId)
    {
        var @event = await _db.Events.FindAsync(eventId);
        if (@event is null) return NotFound();
        if (@event.OrganiserId != User.GetUserId()) return Forbid();

        _db.Events.Remove(@event);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
