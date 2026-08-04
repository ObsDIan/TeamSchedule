namespace TeamSchedule.Models;

public class ActivityResponse
{
    public long ResponseId { get; set; }
    public long ActivityId { get; set; }
    public TeamActivity? Activity { get; set; }
    public long CandidateDateId { get; set; }
    public ActivityCandidateDate? CandidateDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public ActivityResponseStatus ResponseStatus { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
