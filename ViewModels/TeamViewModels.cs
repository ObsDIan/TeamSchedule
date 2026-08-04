using System.ComponentModel.DataAnnotations;
using TeamSchedule.Models;

namespace TeamSchedule.ViewModels;

public class TeamListViewModel
{
    public List<TeamItemViewModel> MyTeams { get; set; } = new();
}

public class TeamItemViewModel
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTeamViewModel
{
    [Required(ErrorMessage = "請輸入團隊名稱")]
    [StringLength(100, ErrorMessage = "團隊名稱長度不能超過 100 個字")]
    public string TeamName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "說明長度不能超過 500 個字")]
    public string? Description { get; set; }
}

public class JoinTeamViewModel
{
    [Required(ErrorMessage = "請輸入團隊邀請碼")]
    [StringLength(50, ErrorMessage = "邀請碼格式不正確")]
    public string InviteCode { get; set; } = string.Empty;
}

public class TeamDetailViewModel
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public List<TeamMemberItemViewModel> Members { get; set; } = new();
    public TeamCalendarMonthViewModel Calendar { get; set; } = new();
    public List<ActivitySummaryViewModel> OngoingActivities { get; set; } = new();
    public List<ActivitySummaryViewModel> ConfirmedActivities { get; set; } = new();
}

public class TeamMemberItemViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TeamRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CreateActivityViewModel
{
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入活動名稱")]
    [StringLength(200, ErrorMessage = "名稱長度不能超過 200 字")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "說明長度不能超過 1000 字")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "請至少選擇一個候選日期")]
    public string CandidateDatesInput { get; set; } = string.Empty; // 逗號分隔或日期字串
}

public class ActivitySummaryViewModel
{
    public long ActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityStatus Status { get; set; }
    public DateTime? FinalDate { get; set; }
    public int CandidateDatesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ActivityDetailViewModel
{
    public long ActivityId { get; set; }
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityStatus Status { get; set; }
    public DateTime? FinalDate { get; set; }
    public bool IsOwner { get; set; }
    public List<CandidateDateItemViewModel> CandidateDates { get; set; } = new();
    public List<ActivityMemberResponseViewModel> MemberResponses { get; set; } = new();
}

public class CandidateDateItemViewModel
{
    public long CandidateDateId { get; set; }
    public DateTime CandidateDate { get; set; }
    public int JoinCount { get; set; }
    public int DeclineCount { get; set; }
    public int MaybeCount { get; set; }
    public ActivityResponseStatus? MyResponse { get; set; }
}

public class ActivityMemberResponseViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Dictionary<long, ActivityResponseStatus> Responses { get; set; } = new();
}

public class RespondActivityRequestModel
{
    public long ActivityId { get; set; }
    public long CandidateDateId { get; set; }
    public ActivityResponseStatus ResponseStatus { get; set; }
}
