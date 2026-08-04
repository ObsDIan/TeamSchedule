namespace TeamSchedule.Models;

public class TeamActivity
{
    public long ActivityId { get; set; }
    public long TeamId { get; set; }
    public Team? Team { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? FinalDate { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Open;
    public string CreatedBy { get; set; } = string.Empty;
    public ApplicationUser? Creator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }

    public ICollection<ActivityCandidateDate> CandidateDates { get; set; } = new List<ActivityCandidateDate>();
    public ICollection<ActivityResponse> Responses { get; set; } = new List<ActivityResponse>();
    public ICollection<ActivityParticipant> Participants { get; set; } = new List<ActivityParticipant>();
}
