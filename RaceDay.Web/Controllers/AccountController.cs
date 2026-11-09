using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts.DTOs;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly IRaceDayApiClient _api;

    public AccountController(IRaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error, response) = await _api.LoginAsync(new LoginRequest
        {
            Email = model.Email,
            Password = model.Password
        });

        if (!success || response is null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Login failed.");
            return View(model);
        }

        _api.SetToken(response.Token);
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, error, response) = await _api.RegisterAsync(new RegisterRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            Role = model.Role
        });

        if (!success || response is null)
        {
            ModelState.AddModelError(string.Empty, error ?? "Registration failed.");
            return View(model);
        }

        _api.SetToken(response.Token);
        return RedirectToAction("Index", "Events");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        _api.ClearToken();
        return RedirectToAction("Index", "Events");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var profile = await _api.GetProfileAsync();
        if (profile is null) return RedirectToAction(nameof(Login));

        ViewData["Enrolments"] = await _api.GetMyEnrolmentsAsync();
        ViewData["Results"] = await _api.GetMyResultsAsync();
        return View(profile);
    }
}
