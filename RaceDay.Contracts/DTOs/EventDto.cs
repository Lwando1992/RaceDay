namespace RaceDay.Contracts.DTOs;

/// <summary>A race event created by an Organiser.</summary>
public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string? Location { get; set; }
    public int OrganiserId { get; set; }
    public string? OrganiserName { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

/// <summary>Payload for creating an event (POST /api/events).</summary>
public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string? Location { get; set; }
}

/// <summary>Payload for updating an event (PUT /api/events/{id}).</summary>
public class UpdateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string? Location { get; set; }
}
