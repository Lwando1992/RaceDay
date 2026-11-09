namespace RaceDay.Contracts.DTOs;

/// <summary>A competition category within an event (e.g. "Senior Male").</summary>
public class CategoryDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>Payload for POST /api/events/{eventId}/categories.</summary>
public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>Payload for PUT /api/categories/{categoryId}.</summary>
public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
