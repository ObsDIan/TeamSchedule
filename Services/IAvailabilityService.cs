using TeamSchedule.Models;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Services;

public interface IAvailabilityService
{
    Task<AvailabilityStatus?> GetFinalStatusAsync(string userId, DateTime date);
    Task<Dictionary<int, AvailabilityStatus>> GetWeeklyAvailabilityAsync(string userId);
    Task<CalendarMonthViewModel> GetPersonalCalendarMonthAsync(string userId, int year, int month);
    Task<TeamCalendarMonthViewModel> GetTeamCalendarMonthAsync(long teamId, int year, int month, string? currentUserId = null);
    Task SetDateOverrideAsync(string userId, DateTime date, AvailabilityStatus? status, string? note = null);
    Task SaveWeeklyAvailabilityAsync(string userId, Dictionary<int, AvailabilityStatus> weeklySettings);
}
