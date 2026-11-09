using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Extensions;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ResultsController(ApplicationDbContext db) => _db = db;

    /// <summary>Capture finish time and position for an enrolment (Organiser only, own events).</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<ResultDto>> Create([FromBody] CreateResultRequest request)
    {
        var enrolment = await _db.Enrolments
            .Include(en => en.Event)
            .Include(en => en.Category)
            .Include(en => en.User)
            .FirstOrDefaultAsync(en => en.Id == request.EnrolmentId);

        if (enrolment is null) return NotFound(new { message = $"Enrolment {request.EnrolmentId} not found." });
        if (enrolment.Event.OrganiserId != User.GetUserId()) return Forbid();
        if (request.FinishTimeSeconds <= 0) return BadRequest(new { message = "Finish time must be positive." });

        var result = await _db.Results.FirstOrDefaultAsync(r => r.EnrolmentId == enrolment.Id);
        if (result is null)
        {
            result = new RaceDay.Api.Models.Result { EnrolmentId = enrolment.Id };
            _db.Results.Add(result);
        }

        result.FinishTimeSeconds = request.FinishTimeSeconds;
        result.Position = request.Position;
        result.RecordedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Created("", result.ToDto());
    }

    /// <summary>Retrieve results for the logged-in participant.</summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetMyResults()
    {
        var userId = User.GetUserId();
        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(en => en.Event)
            .Include(r => r.Enrolment).ThenInclude(en => en.Category)
            .Include(r => r.Enrolment).ThenInclude(en => en.User)
            .Where(r => r.Enrolment.UserId == userId)
            .ToListAsync();

        return Ok(results.Select(r => r.ToDto()));
    }
}
