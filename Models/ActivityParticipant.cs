namespace TeamSchedule.Models;

public class ActivityParticipant
{
    public long ActivityParticipantId { get; set; }
    public long ActivityId { get; set; }
    public TeamActivity? Activity { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public ParticipationStatus ParticipationStatus { get; set; } = ParticipationStatus.Joined;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
