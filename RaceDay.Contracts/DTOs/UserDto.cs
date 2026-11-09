namespace RaceDay.Contracts.DTOs;

/// <summary>Represents a user of the system (Organiser or Participant).</summary>
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Participant";
    public string? ImageUrl { get; set; }
}

/// <summary>Payload for POST /api/auth/register.</summary>
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>"Organiser" or "Participant" (default).</summary>
    public string Role { get; set; } = "Participant";
}

/// <summary>Payload for POST /api/auth/login.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>Returned by register/login. Contains the JWT bearer token.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = new();
}

/// <summary>Payload for PUT /api/users/profile.</summary>
public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
