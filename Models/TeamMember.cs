namespace TeamSchedule.Models;

public class TeamMember
{
    public long TeamMemberId { get; set; }
    public long TeamId { get; set; }
    public Team? Team { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public TeamRole Role { get; set; } = TeamRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
