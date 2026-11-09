namespace RaceDay.Contracts.DTOs;

/// <summary>Links a participant to an event and one of its categories.</summary>
public class EnrolmentDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime EnrolledAtUtc { get; set; }
}

/// <summary>Payload for POST /api/enrolments.</summary>
public class CreateEnrolmentRequest
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}
