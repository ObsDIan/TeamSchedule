using TeamSchedule.Models;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Services;

public interface ITeamService
{
    Task<long> CreateTeamAsync(string userId, string teamName, string? description);
    Task<bool> JoinTeamByInviteCodeAsync(string userId, string inviteCode);
    Task<TeamListViewModel> GetUserTeamsAsync(string userId);
    Task<TeamDetailViewModel> GetTeamDetailAsync(long teamId, string userId, int? year, int? month);
    Task<long> CreateActivityAsync(string userId, long teamId, string title, string? description, List<ActivityCandidateDateInput> candidateDates);
    Task<ActivityDetailViewModel> GetActivityDetailAsync(long activityId, string userId);
    Task RespondActivityAsync(string userId, long activityId, long candidateDateId, ActivityResponseStatus responseStatus);
    Task ConfirmActivityAsync(string userId, long activityId, long candidateDateId);
    Task CancelActivityAsync(string userId, long activityId);
}
