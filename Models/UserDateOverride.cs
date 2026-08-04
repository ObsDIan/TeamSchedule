namespace TeamSchedule.Models;

public class UserDateOverride
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public DateTime TargetDate { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public string? Note { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
