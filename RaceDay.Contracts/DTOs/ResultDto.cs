namespace RaceDay.Contracts.DTOs;

/// <summary>Finish time and position for an enrolment, captured by an Organiser.</summary>
public class ResultDto
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    /// <summary>Finish time in seconds (e.g. 2730.5 = 45:30.5).</summary>
    public double FinishTimeSeconds { get; set; }
    public int Position { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}

/// <summary>Payload for POST /api/results (Organiser only).</summary>
public class CreateResultRequest
{
    public int EnrolmentId { get; set; }
    public double FinishTimeSeconds { get; set; }
    public int Position { get; set; }
}
