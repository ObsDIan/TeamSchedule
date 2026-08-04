using TeamSchedule.Models;

namespace TeamSchedule.ViewModels;

public class WeeklySetupViewModel
{
    public Dictionary<int, AvailabilityStatus> DaySettings { get; set; } = new();

    public static readonly Dictionary<int, string> DayOfWeekNames = new()
    {
        { 1, "星期一" },
        { 2, "星期二" },
        { 3, "星期三" },
        { 4, "星期四" },
        { 5, "星期五" },
        { 6, "星期六" },
        { 0, "星期日" }
    };
}

public class SetDateOverrideRequestModel
{
    public DateTime Date { get; set; }
    public AvailabilityStatus? Status { get; set; }
    public string? Note { get; set; }
}
