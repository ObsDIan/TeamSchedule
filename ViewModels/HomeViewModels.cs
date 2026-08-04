using TeamSchedule.Models;

namespace TeamSchedule.ViewModels;

public class HomeDashboardViewModel
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }

    // Real Metrics
    public int MyTeamsCount { get; set; }
    public int OpenActivitiesCount { get; set; }
    public int ConfirmedActivitiesCount { get; set; }

    // Real Recent Activities
    public List<HomeActivityItemViewModel> RecentActivities { get; set; } = new();

    // Featured Team Calendar Preview (Real data if available)
    public string? FeaturedTeamName { get; set; }
    public string? FeaturedActivityTitle { get; set; }
    public List<HomeCandidatePreviewItem> PreviewCandidateDates { get; set; } = new();
}

public class HomeActivityItemViewModel
{
    public long ActivityId { get; set; }
    public long TeamId { get; set; }
    public string ActivityTitle { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public DateTime? DisplayDate { get; set; }
    public ActivityStatus Status { get; set; }
    public int CandidateCount { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public ActivityResponseStatus? MyResponseStatus { get; set; }
}

public class HomeCandidatePreviewItem
{
    public DateTime Date { get; set; }
    public int AvailableCount { get; set; }
    public int TotalMembersCount { get; set; }
    public double AvailablePercent { get; set; }
    public double BusyPercent { get; set; }
    public double MaybePercent { get; set; }
    public double UnsetPercent { get; set; }
    public bool IsRecommended { get; set; }
}
