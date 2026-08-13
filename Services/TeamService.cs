using Microsoft.EntityFrameworkCore;
using TeamSchedule.Data;
using TeamSchedule.Models;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Services;

public class TeamService(
    ApplicationDbContext context,
    IAvailabilityService availabilityService,
    INotificationService notificationService) : ITeamService
{
    public async Task<long> CreateTeamAsync(string userId, string teamName, string? description)
    {
        string inviteCode = await GenerateUniqueInviteCodeAsync();

        var team = new Team
        {
            TeamName = teamName.Trim(),
            Description = description?.Trim(),
            OwnerUserId = userId,
            InviteCode = inviteCode,
            CreatedAt = DateTime.UtcNow
        };

        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var ownerMember = new TeamMember
        {
            TeamId = team.TeamId,
            UserId = userId,
            Role = TeamRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        context.TeamMembers.Add(ownerMember);
        await context.SaveChangesAsync();

        return team.TeamId;
    }

    public async Task<bool> JoinTeamByInviteCodeAsync(string userId, string inviteCode)
    {
        var cleanCode = inviteCode.Trim().ToUpper();
        var team = await context.Teams
            .FirstOrDefaultAsync(t => t.InviteCode == cleanCode);

        if (team == null) return false;

        var existingMember = await context.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamId == team.TeamId && m.UserId == userId);

        if (existingMember == null)
        {
            context.TeamMembers.Add(new TeamMember
            {
                TeamId = team.TeamId,
                UserId = userId,
                Role = TeamRole.Member,
                JoinedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<TeamListViewModel> GetUserTeamsAsync(string userId)
    {
        var memberShips = await context.TeamMembers
            .Include(m => m.Team)
            .ThenInclude(t => t!.Members)
            .Where(m => m.UserId == userId)
            .ToListAsync();

        var model = new TeamListViewModel();

        foreach (var membership in memberShips)
        {
            if (membership.Team == null) continue;

            model.MyTeams.Add(new TeamItemViewModel
            {
                TeamId = membership.Team.TeamId,
                TeamName = membership.Team.TeamName,
                Description = membership.Team.Description,
                InviteCode = membership.Team.InviteCode,
                IsOwner = membership.Role == TeamRole.Owner || membership.Team.OwnerUserId == userId,
                MemberCount = membership.Team.Members.Count,
                CreatedAt = membership.Team.CreatedAt
            });
        }

        return model;
    }

    public async Task<string> GetTeamNameForMemberAsync(long teamId, string userId)
    {
        var team = await context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("找不到該團隊。");

        var isMember = await context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("您不是該團隊的成員。");
        }

        return team.TeamName;
    }

    public async Task<TeamDetailViewModel> GetTeamDetailAsync(long teamId, string userId, int? year, int? month)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .ThenInclude(m => m.User)
            .Include(t => t.Activities)
            .ThenInclude(a => a.CandidateDates)
            .FirstOrDefaultAsync(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("找不到該團隊。");

        var isMember = team.Members.Any(m => m.UserId == userId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("您不是該團隊的成員。");
        }

        var now = DateTime.Today;
        int targetYear = year ?? now.Year;
        int targetMonth = month ?? now.Month;

        var calendarModel = await availabilityService.GetTeamCalendarMonthAsync(teamId, targetYear, targetMonth, userId);

        var isOwner = team.OwnerUserId == userId || team.Members.Any(m => m.UserId == userId && m.Role == TeamRole.Owner);

        var model = new TeamDetailViewModel
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            Description = team.Description,
            InviteCode = team.InviteCode,
            IsOwner = isOwner,
            Calendar = calendarModel,
            Members = team.Members.Select(m => new TeamMemberItemViewModel
            {
                UserId = m.UserId,
                DisplayName = m.User?.DisplayName ?? m.User?.UserName ?? "未知使用者",
                Email = m.User?.Email ?? "",
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList()
        };

        foreach (var act in team.Activities.OrderByDescending(a => a.CreatedAt))
        {
            var item = new ActivitySummaryViewModel
            {
                ActivityId = act.ActivityId,
                Title = act.Title,
                Description = act.Description,
                Status = act.Status,
                FinalDate = act.FinalDate,
                CandidateDatesCount = act.CandidateDates.Count,
                CreatedAt = act.CreatedAt
            };

            if (act.Status == ActivityStatus.Open)
            {
                model.OngoingActivities.Add(item);
            }
            else if (act.Status == ActivityStatus.Confirmed)
            {
                model.ConfirmedActivities.Add(item);
            }
        }

        return model;
    }

    public async Task<long> CreateActivityAsync(string userId, long teamId, string title, string? description, List<DateTime> candidateDates)
    {
        var isMember = await context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId);
        if (!isMember) throw new UnauthorizedAccessException("權限不足");

        var distinctDates = candidateDates.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
        if (distinctDates.Count == 0)
        {
            throw new InvalidOperationException("請至少提供一個候選日期。");
        }

        if (distinctDates.Count > 30)
        {
            throw new InvalidOperationException("候選日期最多 30 個，請精簡選擇。");
        }

        if (distinctDates.Any(d => d < DateTime.Today))
        {
            throw new InvalidOperationException("候選日期不能是過去的日期，請重新選擇。");
        }

        var activity = new TeamActivity
        {
            TeamId = teamId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = ActivityStatus.Open,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        context.TeamActivities.Add(activity);
        await context.SaveChangesAsync();

        foreach (var d in distinctDates)
        {
            context.ActivityCandidateDates.Add(new ActivityCandidateDate
            {
                ActivityId = activity.ActivityId,
                CandidateDate = d
            });
        }

        await context.SaveChangesAsync();
        return activity.ActivityId;
    }

    public async Task<ActivityDetailViewModel> GetActivityDetailAsync(long activityId, string userId)
    {
        var activity = await context.TeamActivities
            .Include(a => a.Team)
            .ThenInclude(t => t!.Members)
            .ThenInclude(m => m.User)
            .Include(a => a.CandidateDates)
            .ThenInclude(c => c.Responses)
            .FirstOrDefaultAsync(a => a.ActivityId == activityId)
            ?? throw new InvalidOperationException("找不到該活動。");

        var isMember = activity.Team?.Members.Any(m => m.UserId == userId) ?? false;
        if (!isMember) throw new UnauthorizedAccessException("您不是該團隊成員。");

        var isOwner = activity.CreatedBy == userId || (activity.Team?.OwnerUserId == userId);

        var model = new ActivityDetailViewModel
        {
            ActivityId = activity.ActivityId,
            TeamId = activity.TeamId,
            TeamName = activity.Team?.TeamName ?? "",
            Title = activity.Title,
            Description = activity.Description,
            Status = activity.Status,
            FinalDate = activity.FinalDate,
            IsOwner = isOwner
        };

        foreach (var cand in activity.CandidateDates.OrderBy(c => c.CandidateDate))
        {
            var myResp = cand.Responses.FirstOrDefault(r => r.UserId == userId)?.ResponseStatus;

            model.CandidateDates.Add(new CandidateDateItemViewModel
            {
                CandidateDateId = cand.CandidateDateId,
                CandidateDate = cand.CandidateDate,
                JoinCount = cand.Responses.Count(r => r.ResponseStatus == ActivityResponseStatus.Join),
                DeclineCount = cand.Responses.Count(r => r.ResponseStatus == ActivityResponseStatus.Decline),
                MaybeCount = cand.Responses.Count(r => r.ResponseStatus == ActivityResponseStatus.Maybe),
                MyResponse = myResp
            });
        }

        var allMembers = activity.Team?.Members ?? new List<TeamMember>();
        foreach (var mem in allMembers)
        {
            var memResp = new ActivityMemberResponseViewModel
            {
                UserId = mem.UserId,
                DisplayName = mem.User?.DisplayName ?? mem.User?.UserName ?? "成員"
            };

            foreach (var cand in activity.CandidateDates)
            {
                var r = cand.Responses.FirstOrDefault(x => x.UserId == mem.UserId);
                if (r != null)
                {
                    memResp.Responses[cand.CandidateDateId] = r.ResponseStatus;
                }
            }

            model.MemberResponses.Add(memResp);
        }

        return model;
    }

    public async Task RespondActivityAsync(
    string userId,
    long activityId,
    long candidateDateId,
    ActivityResponseStatus responseStatus)
    {
        // 1. 驗證回覆狀態是否為合法列舉值
        if (!Enum.IsDefined(typeof(ActivityResponseStatus), responseStatus))
        {
            throw new InvalidOperationException("無效的活動回覆狀態。");
        }

        // 2. 確認活動存在
        var activity = await context.TeamActivities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ActivityId == activityId)
            ?? throw new InvalidOperationException("找不到指定的活動。");

        // 3. 確認使用者是活動所屬團隊的成員
        var isMember = await context.TeamMembers
            .AnyAsync(m =>
                m.TeamId == activity.TeamId &&
                m.UserId == userId);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("您不是該活動所屬團隊的成員。");
        }

        // 4. 只有尚在調查中的活動可以投票
        if (activity.Status != ActivityStatus.Open)
        {
            throw new InvalidOperationException("此活動已定案或取消，無法再進行回覆。");
        }

        // 5. 確認候選日期確實屬於這個活動
        var candidateExists = await context.ActivityCandidateDates
            .AnyAsync(c =>
                c.ActivityId == activityId &&
                c.CandidateDateId == candidateDateId);

        if (!candidateExists)
        {
            throw new InvalidOperationException("候選日期與活動不一致。");
        }

        // 6. 新增或更新使用者回覆
        var existing = await context.ActivityResponses
            .FirstOrDefaultAsync(r =>
                r.ActivityId == activityId &&
                r.CandidateDateId == candidateDateId &&
                r.UserId == userId);

        if (existing == null)
        {
            context.ActivityResponses.Add(new ActivityResponse
            {
                ActivityId = activityId,
                CandidateDateId = candidateDateId,
                UserId = userId,
                ResponseStatus = responseStatus,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ResponseStatus = responseStatus;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task ConfirmActivityAsync(string userId, long activityId, long candidateDateId)
    {
        var activity = await context.TeamActivities
            .Include(a => a.Team)
            .FirstOrDefaultAsync(a => a.ActivityId == activityId)
            ?? throw new InvalidOperationException("找不到該活動。");

        var isOwner = activity.CreatedBy == userId || (activity.Team?.OwnerUserId == userId);
        if (!isOwner) throw new UnauthorizedAccessException("您沒有權限確認此活動。");

        if (activity.Status == ActivityStatus.Cancelled)
        {
            throw new InvalidOperationException("此活動已取消，無法再進行定案。");
        }

        var candidate = await context.ActivityCandidateDates
            .FirstOrDefaultAsync(c => c.ActivityId == activityId && c.CandidateDateId == candidateDateId)
            ?? throw new InvalidOperationException("無效的候選日期。");

        activity.FinalDate = candidate.CandidateDate;
        activity.Status = ActivityStatus.Confirmed;
        activity.ConfirmedAt = DateTime.UtcNow;

        // Query all members who selected Join for this date
        var joinedUserIds = await context.ActivityResponses
            .Where(r => r.ActivityId == activityId && r.CandidateDateId == candidateDateId && r.ResponseStatus == ActivityResponseStatus.Join)
            .Select(r => r.UserId)
            .ToListAsync();

        // Clear existing participants first to avoid EF Core INSERT-before-DELETE unique index violation
        var oldParticipants = await context.ActivityParticipants
            .Where(p => p.ActivityId == activityId)
            .ToListAsync();
        if (oldParticipants.Any())
        {
            context.ActivityParticipants.RemoveRange(oldParticipants);
            await context.SaveChangesAsync();
        }

        // Add new distinct participants
        var distinctJoinedUserIds = joinedUserIds.Distinct().ToList();
        foreach (var jUserId in distinctJoinedUserIds)
        {
            context.ActivityParticipants.Add(new ActivityParticipant
            {
                ActivityId = activityId,
                UserId = jUserId,
                ParticipationStatus = ParticipationStatus.Joined,
                JoinedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        var recipients = await GetTeamMembersForNotificationAsync(activity.TeamId);
        await notificationService.SendActivityConfirmedAsync(activity, recipients);
    }

    public async Task CancelActivityAsync(string userId, long activityId)
    {
        var activity = await context.TeamActivities
            .Include(a => a.Team)
            .FirstOrDefaultAsync(a => a.ActivityId == activityId)
            ?? throw new InvalidOperationException("找不到該活動。");

        var isOwner = activity.CreatedBy == userId || (activity.Team?.OwnerUserId == userId);
        if (!isOwner) throw new UnauthorizedAccessException("您沒有權限取消此活動。");

        activity.Status = ActivityStatus.Cancelled;
        await context.SaveChangesAsync();

        var recipients = await GetTeamMembersForNotificationAsync(activity.TeamId);
        await notificationService.SendActivityCancelledAsync(activity, recipients);
    }

    public async Task<TeamActivity> GetActivityForExportAsync(long activityId, string userId)
    {
        var activity = await context.TeamActivities
            .Include(a => a.Team)
            .ThenInclude(t => t!.Members)
            .FirstOrDefaultAsync(a => a.ActivityId == activityId)
            ?? throw new InvalidOperationException("找不到該活動。");

        var isMember = activity.Team?.Members.Any(m => m.UserId == userId) ?? false;
        if (!isMember) throw new UnauthorizedAccessException("您不是該團隊成員。");

        if (activity.Status != ActivityStatus.Confirmed || !activity.FinalDate.HasValue)
        {
            throw new InvalidOperationException("此活動尚未定案，無法匯出行事曆。");
        }

        return activity;
    }

    private async Task<IReadOnlyList<ApplicationUser>> GetTeamMembersForNotificationAsync(long teamId)
    {
        return await context.TeamMembers
            .Where(m => m.TeamId == teamId)
            .Select(m => m.User!)
            .ToListAsync();
    }

    private async Task<string> GenerateUniqueInviteCodeAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        string code;
        bool isUnique;

        do
        {
            code = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            isUnique = !await context.Teams.AnyAsync(t => t.InviteCode == code);
        } while (!isUnique);

        return code;
    }
}
