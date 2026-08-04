using Microsoft.EntityFrameworkCore;
using TeamSchedule.Data;
using TeamSchedule.Models;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Services;

public class AvailabilityService(ApplicationDbContext context) : IAvailabilityService
{
    public async Task<AvailabilityStatus?> GetFinalStatusAsync(string userId, DateTime date)
    {
        var dateOnly = date.Date;

        // Rule 1: Confirmed activity
        var hasConfirmedActivity = await context.ActivityParticipants
            .Include(p => p.Activity)
            .AnyAsync(p => p.UserId == userId &&
                           p.ParticipationStatus == ParticipationStatus.Joined &&
                           p.Activity != null &&
                           p.Activity.Status == ActivityStatus.Confirmed &&
                           p.Activity.FinalDate.HasValue &&
                           p.Activity.FinalDate.Value.Date == dateOnly);

        if (hasConfirmedActivity)
        {
            return AvailabilityStatus.Busy;
        }

        // Rule 2: Date override
        var dateOverride = await context.UserDateOverrides
            .FirstOrDefaultAsync(o => o.UserId == userId && o.TargetDate.Date == dateOnly);

        if (dateOverride != null)
        {
            return dateOverride.AvailabilityStatus;
        }

        // Rule 3: Weekly default
        var dayOfWeek = (int)dateOnly.DayOfWeek;
        var weeklySetting = await context.UserWeeklyAvailabilities
            .FirstOrDefaultAsync(w => w.UserId == userId && w.DayOfWeek == dayOfWeek);

        if (weeklySetting != null)
        {
            return weeklySetting.AvailabilityStatus;
        }

        // Rule 4: Unset (null)
        return null;
    }

    public async Task<CalendarMonthViewModel> GetPersonalCalendarMonthAsync(string userId, int year, int month)
    {
        var firstDayOfMonth = new DateTime(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        int previousDays = (int)firstDayOfMonth.DayOfWeek;
        var calendarStart = firstDayOfMonth.AddDays(-previousDays);

        int nextDays = 6 - (int)lastDayOfMonth.DayOfWeek;
        var calendarEnd = lastDayOfMonth.AddDays(nextDays);

        // Batch query weekly availabilities
        var weeklyMap = await context.UserWeeklyAvailabilities
            .Where(w => w.UserId == userId)
            .ToDictionaryAsync(w => w.DayOfWeek, w => w.AvailabilityStatus);

        // Batch query date overrides
        var overridesMap = await context.UserDateOverrides
            .Where(o => o.UserId == userId && o.TargetDate >= calendarStart && o.TargetDate <= calendarEnd)
            .ToDictionaryAsync(o => o.TargetDate.Date);

        // Batch query confirmed activities
        var confirmedActivities = await context.ActivityParticipants
            .Include(p => p.Activity)
            .Where(p => p.UserId == userId &&
                        p.ParticipationStatus == ParticipationStatus.Joined &&
                        p.Activity != null &&
                        p.Activity.Status == ActivityStatus.Confirmed &&
                        p.Activity.FinalDate.HasValue &&
                        p.Activity.FinalDate.Value >= calendarStart &&
                        p.Activity.FinalDate.Value <= calendarEnd)
            .ToDictionaryAsync(p => p.Activity!.FinalDate!.Value.Date, p => p.Activity!.Title);

        var model = new CalendarMonthViewModel
        {
            Year = year,
            Month = month
        };

        for (var date = calendarStart.Date; date <= calendarEnd.Date; date = date.AddDays(1))
        {
            var dayModel = new CalendarDayViewModel
            {
                Date = date,
                IsCurrentMonth = date.Month == month,
                IsToday = date.Date == DateTime.Today
            };

            if (confirmedActivities.TryGetValue(date, out var activityTitle))
            {
                dayModel.Status = AvailabilityStatus.Busy;
                dayModel.IsConfirmedActivityBusy = true;
                dayModel.ConfirmedActivityTitle = activityTitle;
            }
            else if (overridesMap.TryGetValue(date, out var dateOverride))
            {
                dayModel.Status = dateOverride.AvailabilityStatus;
                dayModel.Note = dateOverride.Note;
            }
            else if (weeklyMap.TryGetValue((int)date.DayOfWeek, out var weeklyStatus))
            {
                dayModel.Status = weeklyStatus;
            }
            else
            {
                dayModel.Status = null;
            }

            model.Days.Add(dayModel);
        }

        return model;
    }

    public async Task<TeamCalendarMonthViewModel> GetTeamCalendarMonthAsync(long teamId, int year, int month, string? currentUserId = null)
    {
        var team = await context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException($"Team {teamId} not found.");

        var memberUserIds = team.Members.Select(m => m.UserId).ToList();
        int totalMemberCount = memberUserIds.Count;

        var firstDayOfMonth = new DateTime(year, month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        int previousDays = (int)firstDayOfMonth.DayOfWeek;
        var calendarStart = firstDayOfMonth.AddDays(-previousDays);

        int nextDays = 6 - (int)lastDayOfMonth.DayOfWeek;
        var calendarEnd = lastDayOfMonth.AddDays(nextDays);

        // Weekly availabilities for all members
        var weeklyAvailabilities = await context.UserWeeklyAvailabilities
            .Where(w => memberUserIds.Contains(w.UserId))
            .ToListAsync();

        // Date overrides for all members in date range
        var dateOverrides = await context.UserDateOverrides
            .Where(o => memberUserIds.Contains(o.UserId) && o.TargetDate >= calendarStart && o.TargetDate <= calendarEnd)
            .ToListAsync();

        // Confirmed activities for all members in date range
        var confirmedActivities = await context.ActivityParticipants
            .Include(p => p.Activity)
            .Where(p => memberUserIds.Contains(p.UserId) &&
                        p.ParticipationStatus == ParticipationStatus.Joined &&
                        p.Activity != null &&
                        p.Activity.Status == ActivityStatus.Confirmed &&
                        p.Activity.FinalDate.HasValue &&
                        p.Activity.FinalDate.Value >= calendarStart &&
                        p.Activity.FinalDate.Value <= calendarEnd)
            .ToListAsync();

        // Candidate dates for OPEN team activities
        var candidateDates = await context.ActivityCandidateDates
            .Include(c => c.Activity)
            .Where(c => c.Activity != null &&
                        c.Activity.TeamId == teamId &&
                        c.Activity.Status == ActivityStatus.Open &&
                        c.CandidateDate >= calendarStart &&
                        c.CandidateDate <= calendarEnd)
            .ToListAsync();

        // Confirmed team activities on final date
        var confirmedTeamActivities = await context.TeamActivities
            .Where(a => a.TeamId == teamId &&
                        a.Status == ActivityStatus.Confirmed &&
                        a.FinalDate.HasValue &&
                        a.FinalDate.Value >= calendarStart &&
                        a.FinalDate.Value <= calendarEnd)
            .ToListAsync();

        var model = new TeamCalendarMonthViewModel
        {
            TeamId = teamId,
            TeamName = team.TeamName,
            Year = year,
            Month = month,
            TotalMembersCount = totalMemberCount
        };

        for (var date = calendarStart.Date; date <= calendarEnd.Date; date = date.AddDays(1))
        {
            int dayOfWeek = (int)date.DayOfWeek;

            int availableCount = 0;
            int busyCount = 0;
            int maybeCount = 0;
            int unsetCount = 0;

            AvailabilityStatus? currentUserStatusOnDate = null;

            foreach (var userId in memberUserIds)
            {
                AvailabilityStatus? userStatus = null;

                // Rule 1: Confirmed activity
                var hasConfirmed = confirmedActivities.Any(p => p.UserId == userId && p.Activity?.FinalDate?.Date == date);
                if (hasConfirmed)
                {
                    userStatus = AvailabilityStatus.Busy;
                }
                else
                {
                    // Rule 2: Override
                    var userOverride = dateOverrides.FirstOrDefault(o => o.UserId == userId && o.TargetDate.Date == date);
                    if (userOverride != null)
                    {
                        userStatus = userOverride.AvailabilityStatus;
                    }
                    else
                    {
                        // Rule 3: Weekly
                        var userWeekly = weeklyAvailabilities.FirstOrDefault(w => w.UserId == userId && w.DayOfWeek == dayOfWeek);
                        if (userWeekly != null)
                        {
                            userStatus = userWeekly.AvailabilityStatus;
                        }
                    }
                }

                if (currentUserId != null && userId == currentUserId)
                {
                    currentUserStatusOnDate = userStatus;
                }

                switch (userStatus)
                {
                    case AvailabilityStatus.Available:
                        availableCount++;
                        break;
                    case AvailabilityStatus.Busy:
                        busyCount++;
                        break;
                    case AvailabilityStatus.Maybe:
                        maybeCount++;
                        break;
                    default:
                        unsetCount++;
                        break;
                }
            }

            var confirmedAct = confirmedTeamActivities.FirstOrDefault(a => a.FinalDate?.Date == date);
            var dayCandidate = candidateDates.FirstOrDefault(c => c.CandidateDate.Date == date);

            var dayModel = new TeamCalendarDayViewModel
            {
                Date = date,
                IsCurrentMonth = date.Month == month,
                IsToday = date.Date == DateTime.Today,
                AvailableCount = availableCount,
                BusyCount = busyCount,
                MaybeCount = maybeCount,
                UnsetCount = unsetCount,
                TotalMemberCount = totalMemberCount,
                MyStatus = currentUserStatusOnDate,
                IsConfirmedDate = confirmedAct != null,
                IsCandidateDate = confirmedAct == null && dayCandidate != null,
                ActivityId = confirmedAct?.ActivityId ?? dayCandidate?.ActivityId,
                ActivityTitle = confirmedAct?.Title ?? dayCandidate?.Activity?.Title
            };

            model.Days.Add(dayModel);
        }

        return model;
    }

    public async Task SetDateOverrideAsync(string userId, DateTime date, AvailabilityStatus? status, string? note = null)
    {
        var dateOnly = date.Date;
        var existing = await context.UserDateOverrides
            .FirstOrDefaultAsync(o => o.UserId == userId && o.TargetDate.Date == dateOnly);

        if (status == null)
        {
            if (existing != null)
            {
                context.UserDateOverrides.Remove(existing);
            }
        }
        else
        {
            if (existing == null)
            {
                context.UserDateOverrides.Add(new UserDateOverride
                {
                    UserId = userId,
                    TargetDate = dateOnly,
                    AvailabilityStatus = status.Value,
                    Note = note,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.AvailabilityStatus = status.Value;
                existing.Note = note;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task SaveWeeklyAvailabilityAsync(string userId, Dictionary<int, AvailabilityStatus> weeklySettings)
    {
        var existingSettings = await context.UserWeeklyAvailabilities
            .Where(w => w.UserId == userId)
            .ToDictionaryAsync(w => w.DayOfWeek);

        foreach (var (dayOfWeek, status) in weeklySettings)
        {
            if (existingSettings.TryGetValue(dayOfWeek, out var setting))
            {
                setting.AvailabilityStatus = status;
                setting.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                context.UserWeeklyAvailabilities.Add(new UserWeeklyAvailability
                {
                    UserId = userId,
                    DayOfWeek = dayOfWeek,
                    AvailabilityStatus = status,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
