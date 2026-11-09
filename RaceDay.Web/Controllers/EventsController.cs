using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts.DTOs;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly IRaceDayApiClient _api;

    public EventsController(IRaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.IsOrganiser = (await _api.GetProfileAsync())?.Role == "Organiser";
        return View(await _api.GetEventsAsync());
    }

    public async Task<IActionResult> Details(int id, string? error = null)
    {
        var @event = await _api.GetEventAsync(id);
        if (@event is null) return NotFound();

        if (!string.IsNullOrEmpty(error)) ModelState.AddModelError(string.Empty, error);
        ViewBag.IsOrganiser = (await _api.GetProfileAsync())?.Role == "Organiser";
        ViewBag.IsAuthenticated = await _api.IsAuthenticatedAsync();
        return View(@event);
    }

    [HttpGet]
    public IActionResult Create() => View(new EventFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error) = await _api.CreateEventAsync(new CreateEventRequest
        {
            Name = model.Name,
            Description = model.Description,
            Date = model.Date,
            Location = model.Location
        });

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Could not create the event.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (_, error) = await _api.DeleteEventAsync(id);
        if (error is not null) TempData["Error"] = error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int id, int categoryId)
    {
        var (success, error) = await _api.EnrolAsync(id, categoryId);
        if (!success)
        {
            return RedirectToAction(nameof(Details), new { id, error });
        }

        TempData["Message"] = "You are enrolled - see My Enrolments.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int id, string name, string? description)
    {
        var (success, error) = await _api.CreateCategoryAsync(id, new CreateCategoryRequest
        {
            Name = name,
            Description = description
        });

        if (!success && error is not null) TempData["Error"] = error;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResult(int id, int enrolmentId, double finishTimeSeconds, int position)
    {
        var (success, error) = await _api.CreateResultAsync(enrolmentId, finishTimeSeconds, position);
        if (!success && error is not null) TempData["Error"] = error;
        else TempData["Message"] = "Result captured.";
        return RedirectToAction(nameof(Entrants), new { id });
    }

    public async Task<IActionResult> Enrolments()
    {
        if (!await _api.IsAuthenticatedAsync()) return RedirectToAction("Login", "Account");
        return View(await _api.GetMyEnrolmentsAsync());
    }

    public async Task<IActionResult> Results()
    {
        if (!await _api.IsAuthenticatedAsync()) return RedirectToAction("Login", "Account");
        return View(await _api.GetMyResultsAsync());
    }

    /// <summary>Organiser view: entrants for one of my events.</summary>
    public async Task<IActionResult> Entrants(int id)
    {
        if (!await _api.IsAuthenticatedAsync()) return RedirectToAction("Login", "Account");

        ViewBag.EventId = id;
        return View(await _api.GetEventEnrolmentsAsync(id));
    }
}
