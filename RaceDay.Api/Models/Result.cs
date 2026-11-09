namespace RaceDay.Api.Models;

public class Result
{
    public int Id { get; set; }

    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;

    /// <summary>Finish time in seconds.</summary>
    public double FinishTimeSeconds { get; set; }

    public int Position { get; set; }

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}
