using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using RaceDay.Contracts.DTOs;

namespace RaceDay.Web.Services;

/// <summary>Typed client that consumes the RaceDay API on behalf of the MVC app.</summary>
public interface IRaceDayApiClient
{
    Task<bool> IsAuthenticatedAsync();
    string? GetTokenAsync();
    void SetToken(string token);
    void ClearToken();

    Task<(bool Success, string? Error, AuthResponse? Response)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string? Error, AuthResponse? Response)> LoginAsync(LoginRequest request);
    Task<UserDto?> GetProfileAsync();
    Task<List<EventDto>> GetEventsAsync();
    Task<EventDto?> GetEventAsync(int eventId);
    Task<(bool Success, string? Error)> CreateEventAsync(CreateEventRequest request);
    Task<(bool Success, string? Error)> UpdateEventAsync(int eventId, UpdateEventRequest request);
    Task<(bool Success, string? Error)> DeleteEventAsync(int eventId);
    Task<(bool Success, string? Error)> CreateCategoryAsync(int eventId, CreateCategoryRequest request);
    Task<(bool Success, string? Error)> EnrolAsync(int eventId, int categoryId);
    Task<List<EnrolmentDto>> GetMyEnrolmentsAsync();
    Task<List<EnrolmentDto>> GetEventEnrolmentsAsync(int eventId);
    Task<(bool Success, string? Error)> CreateResultAsync(int enrolmentId, double finishTimeSeconds, int position);
    Task<List<ResultDto>> GetMyResultsAsync();
}

public class RaceDayApiClient : IRaceDayApiClient
{
    private const string TokenKey = "RaceDay.Jwt";

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    public Task<bool> IsAuthenticatedAsync() => Task.FromResult(!string.IsNullOrEmpty(GetTokenAsync()));

    public string? GetTokenAsync() => _accessor.HttpContext?.Session.GetString(TokenKey);

    public void SetToken(string token) => _accessor.HttpContext?.Session.SetString(TokenKey, token);

    public void ClearToken() => _accessor.HttpContext?.Session.Remove(TokenKey);

    private void AttachToken()
    {
        var token = GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = token is null
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    private static string ReadError(HttpResponseMessage response) =>
        response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Invalid email or password.",
            System.Net.HttpStatusCode.Forbidden => "You are not allowed to perform this action.",
            System.Net.HttpStatusCode.NotFound => "The requested item was not found.",
            System.Net.HttpStatusCode.Conflict => "You are already enrolled in this event.",
            _ => $"The API returned {(int)response.StatusCode} ({response.StatusCode})."
        };

    public async Task<(bool, string?, AuthResponse?)> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        if (!response.IsSuccessStatusCode) return (false, ReadError(response), null);
        return (true, null, await response.Content.ReadFromJsonAsync<AuthResponse>());
    }

    public async Task<(bool, string?, AuthResponse?)> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return (false, ReadError(response), null);
        return (true, null, await response.Content.ReadFromJsonAsync<AuthResponse>());
    }

    public async Task<UserDto?> GetProfileAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("api/users/profile");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task<List<EventDto>> GetEventsAsync() =>
        await _http.GetFromJsonAsync<List<EventDto>>("api/events") ?? new();

    public async Task<EventDto?> GetEventAsync(int eventId) =>
        await _http.GetFromJsonAsync<EventDto>($"api/events/{eventId}");

    public async Task<(bool, string?)> CreateEventAsync(CreateEventRequest request)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("api/events", request);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : ReadError(response));
    }

    public async Task<(bool, string?)> UpdateEventAsync(int eventId, UpdateEventRequest request)
    {
        AttachToken();
        var response = await _http.PutAsJsonAsync($"api/events/{eventId}", request);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : ReadError(response));
    }

    public async Task<(bool, string?)> DeleteEventAsync(int eventId)
    {
        AttachToken();
        var response = await _http.DeleteAsync($"api/events/{eventId}");
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : ReadError(response));
    }

    public async Task<(bool, string?)> CreateCategoryAsync(int eventId, CreateCategoryRequest request)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync($"api/events/{eventId}/categories", request);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : ReadError(response));
    }

    public async Task<(bool, string?)> EnrolAsync(int eventId, int categoryId)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("api/enrolments", new CreateEnrolmentRequest
        {
            EventId = eventId,
            CategoryId = categoryId
        });
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : ReadError(response));
    }

    public async Task<List<EnrolmentDto>> GetMyEnrolmentsAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("api/enrolments");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<EnrolmentDto>>() ?? new();
    }

    public async Task<List<EnrolmentDto>> GetEventEnrolmentsAsync(int eventId)
    {
        AttachToken();
        var response = await _http.GetAsync($"api/events/{eventId}/enrolments");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<EnrolmentDto>>() ?? new();
    }

    public async Task<(bool, string?)> CreateResultAsync(int enrolmentId, double finishTimeSeconds, int position)
    {
        AttachToken();
        var response = await _http.PostAsJsonAsync("api/results", new CreateResultRequest
        {
            EnrolmentId = enrolmentId,
            FinishTimeSeconds = finishTimeSeconds,
            Position = position
        });
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? null : ReadError(response));
    }

    public async Task<List<ResultDto>> GetMyResultsAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("api/results");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<ResultDto>>() ?? new();
    }
}
