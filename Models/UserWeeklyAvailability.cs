namespace TeamSchedule.Models;

public class UserWeeklyAvailability
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public int DayOfWeek { get; set; } // 0 至 6 (Sunday to Saturday)
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
