using TeamSchedule.Models;

namespace TeamSchedule.ViewModels;

public class CalendarMonthViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthTitle => $"{Year} 年 {Month} 月";
    public List<CalendarDayViewModel> Days { get; set; } = new();
}

public class CalendarDayViewModel
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public AvailabilityStatus? Status { get; set; }
    public string? Note { get; set; }
    public bool IsConfirmedActivityBusy { get; set; }
    public string? ConfirmedActivityTitle { get; set; }
}

public class TeamCalendarMonthViewModel
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthTitle => $"{Year} 年 {Month} 月";
    public int TotalMembersCount { get; set; }
    public List<TeamCalendarDayViewModel> Days { get; set; } = new();
}

public class TeamCalendarDayViewModel
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }

    public int AvailableCount { get; set; }
    public int BusyCount { get; set; }
    public int MaybeCount { get; set; }
    public int UnsetCount { get; set; }
    public int TotalMemberCount { get; set; }

    public AvailabilityStatus? MyStatus { get; set; }
    public bool IsCandidateDate { get; set; }
    public bool IsConfirmedDate { get; set; }
    public long? ActivityId { get; set; }
    public string? ActivityTitle { get; set; }
    public string ActivityTimeText { get; set; } = string.Empty;

    public double AvailablePercent => CalculatePercent(AvailableCount);
    public double BusyPercent => CalculatePercent(BusyCount);
    public double MaybePercent => CalculatePercent(MaybeCount);
    public double UnsetPercent => CalculatePercent(UnsetCount);

    private double CalculatePercent(int count)
    {
        if (TotalMemberCount <= 0) return 0;
        return Math.Round((double)count / TotalMemberCount * 100, 2);
    }
}
