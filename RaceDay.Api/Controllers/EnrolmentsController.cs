using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EnrolmentsController(ApplicationDbContext db) => _db = db;

    /// <summary>Enrol the logged-in participant in an event and select a category.</summary>
    [HttpPost("api/enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<EnrolmentDto>> Enrol([FromBody] CreateEnrolmentRequest request)
    {
        var @event = await _db.Events
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.Id == request.EventId);
        if (@event is null) return NotFound(new { message = $"Event {request.EventId} not found." });

        var category = @event.Categories.FirstOrDefault(c => c.Id == request.CategoryId);
        if (category is null)
            return BadRequest(new { message = $"Category {request.CategoryId} does not belong to event {request.EventId}." });

        var userId = User.GetUserId();
        var alreadyEnrolled = await _db.Enrolments.AnyAsync(en => en.EventId == request.EventId && en.UserId == userId);
        if (alreadyEnrolled)
            return Conflict(new { message = "You are already enrolled in this event." });

        var enrolment = new RaceDay.Api.Models.Enrolment
        {
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        enrolment = await Load(enrolment.Id);
        return Created("", enrolment!.ToDto());
    }

    /// <summary>Retrieve all enrolments for the logged-in participant.</summary>
    [HttpGet("api/enrolments")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetMyEnrolments()
    {
        var userId = User.GetUserId();
        var enrolments = await _db.Enrolments
            .Include(en => en.Event)
            .Include(en => en.Category)
            .Include(en => en.User)
            .Where(en => en.UserId == userId)
            .OrderBy(en => en.Event.Date)
            .ToListAsync();

        return Ok(enrolments.Select(en => en.ToDto()));
    }

    /// <summary>View enrolments for a specific event (Organiser only, own events).</summary>
    [HttpGet("api/events/{eventId:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetEventEnrolments(int eventId)
    {
        var @event = await _db.Events.FindAsync(eventId);
        if (@event is null) return NotFound();
        if (@event.OrganiserId != User.GetUserId()) return Forbid();

        var enrolments = await _db.Enrolments
            .Include(en => en.Event)
            .Include(en => en.Category)
            .Include(en => en.User)
            .Where(en => en.EventId == eventId)
            .ToListAsync();

        return Ok(enrolments.Select(en => en.ToDto()));
    }

    private Task<RaceDay.Api.Models.Enrolment?> Load(int id) =>
        _db.Enrolments
            .Include(en => en.Event)
            .Include(en => en.Category)
            .Include(en => en.User)
            .FirstOrDefaultAsync(en => en.Id == id);
}
