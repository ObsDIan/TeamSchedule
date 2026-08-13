using TeamSchedule.Models;

namespace TeamSchedule.Services;

public interface INotificationService
{
    Task SendActivityConfirmedAsync(TeamActivity activity, IReadOnlyList<ApplicationUser> recipients);
    Task SendActivityCancelledAsync(TeamActivity activity, IReadOnlyList<ApplicationUser> recipients);
}
