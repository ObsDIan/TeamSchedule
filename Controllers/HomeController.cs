using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamSchedule.Data;
using TeamSchedule.Models;
using TeamSchedule.Services;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAvailabilityService _availabilityService;

    public HomeController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IAvailabilityService availabilityService)
    {
        _context = context;
        _userManager = userManager;
        _availabilityService = availabilityService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeDashboardViewModel
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        };

        if (model.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.UserName = user.DisplayName ?? user.UserName;

                // 1. 查詢使用者加入的團隊
                var joinedTeamIds = await _context.TeamMembers
                    .Where(m => m.UserId == user.Id)
                    .Select(m => m.TeamId)
                    .ToListAsync();

                model.MyTeamsCount = joinedTeamIds.Count;

                // 2. 查詢加入團隊中的進行中與已確定活動數量
                var teamActivities = await _context.TeamActivities
                    .Include(a => a.Team)
                    .Include(a => a.CandidateDates)
                    .Where(a => joinedTeamIds.Contains(a.TeamId))
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                model.OpenActivitiesCount = teamActivities.Count(a => a.Status == ActivityStatus.Open);
                model.ConfirmedActivitiesCount = teamActivities.Count(a => a.Status == ActivityStatus.Confirmed);

                // 3. 查詢真實近期活動動態 (前 5 筆)
                var candidateIds = teamActivities.SelectMany(a => a.CandidateDates).Select(c => c.CandidateDateId).ToList();
                var userResponses = await _context.ActivityResponses
                    .Where(r => r.UserId == user.Id && candidateIds.Contains(r.CandidateDateId))
                    .ToListAsync();

                foreach (var act in teamActivities.Take(5))
                {
                    ActivityResponseStatus? myRespStatus = null;
                    var actCandIds = act.CandidateDates.Select(c => c.CandidateDateId).ToList();
                    var myResp = userResponses.FirstOrDefault(r => actCandIds.Contains(r.CandidateDateId));
                    if (myResp != null)
                    {
                        myRespStatus = myResp.ResponseStatus;
                    }

                    model.RecentActivities.Add(new HomeActivityItemViewModel
                    {
                        ActivityId = act.ActivityId,
                        TeamId = act.TeamId,
                        ActivityTitle = act.Title,
                        TeamName = act.Team?.TeamName ?? "團隊",
                        DisplayDate = act.Status == ActivityStatus.Confirmed ? act.FinalDate : act.CandidateDates.FirstOrDefault()?.CandidateDate,
                        Status = act.Status,
                        CandidateCount = act.CandidateDates.Count,
                        MyResponseStatus = myRespStatus,
                        StatusText = act.Status == ActivityStatus.Confirmed ? "已確認" : (myRespStatus.HasValue ? "已回覆" : "等待回覆")
                    });
                }

                // 4. 抓取真實第一個團隊的 4 色填充月曆預覽 (如有團隊與活動)
                if (joinedTeamIds.Any())
                {
                    var featuredTeamId = joinedTeamIds.First();
                    var featuredTeam = await _context.Teams.FirstOrDefaultAsync(t => t.TeamId == featuredTeamId);
                    var featuredAct = teamActivities.FirstOrDefault(a => a.TeamId == featuredTeamId);

                    if (featuredTeam != null)
                    {
                        model.FeaturedTeamName = featuredTeam.TeamName;
                        model.FeaturedActivityTitle = featuredAct?.Title ?? "團隊活動排程";

                        var monthCalendar = await _availabilityService.GetTeamCalendarMonthAsync(featuredTeamId, DateTime.Today.Year, DateTime.Today.Month, user.Id);
                        var upcomingDays = monthCalendar.Days.Where(d => d.Date >= DateTime.Today).Take(4).ToList();

                        if (upcomingDays.Any())
                        {
                            var maxAvailable = upcomingDays.Max(d => d.AvailableCount);

                            foreach (var d in upcomingDays)
                            {
                                model.PreviewCandidateDates.Add(new HomeCandidatePreviewItem
                                {
                                    Date = d.Date,
                                    AvailableCount = d.AvailableCount,
                                    TotalMembersCount = d.TotalMemberCount,
                                    AvailablePercent = d.AvailablePercent,
                                    BusyPercent = d.BusyPercent,
                                    MaybePercent = d.MaybePercent,
                                    UnsetPercent = d.UnsetPercent,
                                    IsRecommended = d.AvailableCount > 0 && d.AvailableCount == maxAvailable
                                });
                            }
                        }
                    }
                }
            }
        }

        // 若無團隊資料或未登入，提供預設質感範例預覽
        if (!model.PreviewCandidateDates.Any())
        {
            model.FeaturedTeamName = "羽球社";
            model.FeaturedActivityTitle = "八月聚會";
            var baseDate = DateTime.Today;

            model.PreviewCandidateDates = new List<HomeCandidatePreviewItem>
            {
                new() { Date = baseDate.AddDays(2), AvailableCount = 5, TotalMembersCount = 10, AvailablePercent = 50, BusyPercent = 20, MaybePercent = 10, UnsetPercent = 20, IsRecommended = false },
                new() { Date = baseDate.AddDays(3), AvailableCount = 8, TotalMembersCount = 10, AvailablePercent = 80, BusyPercent = 10, MaybePercent = 10, UnsetPercent = 0, IsRecommended = true },
                new() { Date = baseDate.AddDays(9), AvailableCount = 6, TotalMembersCount = 10, AvailablePercent = 60, BusyPercent = 30, MaybePercent = 0, UnsetPercent = 10, IsRecommended = false },
                new() { Date = baseDate.AddDays(10), AvailableCount = 4, TotalMembersCount = 10, AvailablePercent = 40, BusyPercent = 40, MaybePercent = 10, UnsetPercent = 10, IsRecommended = false }
            };
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
