using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeamSchedule.Models;
using TeamSchedule.Services;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Controllers;

[Authorize]
public class TeamController(
    ITeamService teamService,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var model = await teamService.GetUserTeamsAsync(userId);
        return View(model);
    }

    public IActionResult Create()
    {
        return View(new CreateTeamViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeamViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var teamId = await teamService.CreateTeamAsync(userId, model.TeamName, model.Description);
        TempData["SuccessMessage"] = "成功建立團隊！";

        return RedirectToAction(nameof(Detail), new { id = teamId });
    }

    public IActionResult Join()
    {
        return View(new JoinTeamViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(JoinTeamViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var success = await teamService.JoinTeamByInviteCodeAsync(userId, model.InviteCode);
        if (!success)
        {
            ModelState.AddModelError(nameof(model.InviteCode), "無效的團隊邀請碼，請再次確認。");
            return View(model);
        }

        TempData["SuccessMessage"] = "已成功加入團隊！";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(long id, int? year, int? month)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            var model = await teamService.GetTeamDetailAsync(id, userId, year, month);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> CreateActivity(long teamId)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var team = await teamService.GetTeamDetailAsync(teamId, userId, null, null);
        var model = new CreateActivityViewModel
        {
            TeamId = teamId,
            TeamName = team.TeamName
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateActivity(CreateActivityViewModel model)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        if (string.IsNullOrWhiteSpace(model.CandidateDatesInput))
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), "請至少輸入一個候選日期。");
        }

        if (!ModelState.IsValid) return View(model);

        var candidates = new List<ActivityCandidateDateInput>();
        var parts = model.CandidateDatesInput.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            var candidate = ParseCandidateInput(p.Trim());
            if (candidate == null)
            {
                ModelState.AddModelError(nameof(model.CandidateDatesInput), "無法解析「" + p.Trim() + "」，請使用 YYYY-MM-DD 或 YYYY-MM-DD HH:mm~HH:mm 格式。");
                return View(model);
            }

            if (candidate.EndTime.HasValue && !candidate.StartTime.HasValue)
            {
                ModelState.AddModelError(nameof(model.CandidateDatesInput), "「" + p.Trim() + "」的結束時間需搭配開始時間。");
                return View(model);
            }

            if (candidate.StartTime.HasValue && candidate.EndTime.HasValue && candidate.EndTime <= candidate.StartTime)
            {
                ModelState.AddModelError(nameof(model.CandidateDatesInput), "「" + p.Trim() + "」的結束時間必須晚於開始時間。");
                return View(model);
            }

            candidates.Add(candidate);
        }

        if (candidates.Count == 0)
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), "無法解析日期格式，請使用 YYYY-MM-DD 或 YYYY-MM-DD HH:mm~HH:mm 格式。");
            return View(model);
        }

        var activityId = await teamService.CreateActivityAsync(userId, model.TeamId, model.Title, model.Description, candidates);
        TempData["SuccessMessage"] = "已成功建立團隊活動！";

        return RedirectToAction(nameof(ActivityDetail), new { id = activityId });
    }

    private static ActivityCandidateDateInput? ParseCandidateInput(string input)
    {
        var segments = input.Split(new[] { '~', '～' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 1)
        {
            if (!DateTime.TryParse(segments[0].Trim(), out var date)) return null;
            return new ActivityCandidateDateInput
            {
                Date = date.Date,
                StartTime = date.TimeOfDay == TimeSpan.Zero ? null : date.TimeOfDay,
                EndTime = null
            };
        }

        if (segments.Length == 2)
        {
            if (!DateTime.TryParse(segments[0].Trim(), out var date)) return null;
            if (!TimeSpan.TryParse(segments[1].Trim(), out var endTime)) return null;
            return new ActivityCandidateDateInput
            {
                Date = date.Date,
                StartTime = date.TimeOfDay == TimeSpan.Zero ? null : date.TimeOfDay,
                EndTime = endTime
            };
        }

        return null;
    }

    public async Task<IActionResult> ActivityDetail(long id)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            var model = await teamService.GetActivityDetailAsync(id, userId);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondActivity(
    [FromBody] RespondActivityRequestModel? request)
    {
        var userId = userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new
            {
                success = false,
                message = "登入狀態已失效，請重新登入。"
            });
        }

        if (request == null ||
            request.ActivityId <= 0 ||
            request.CandidateDateId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "活動投票資料不完整。"
            });
        }

        try
        {
            await teamService.RespondActivityAsync(
                userId,
                request.ActivityId,
                request.CandidateDateId,
                request.ResponseStatus);

            return Json(new
            {
                success = true
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    success = false,
                    message = ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmActivity(long activityId, long candidateDateId)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        await teamService.ConfirmActivityAsync(userId, activityId, candidateDateId);
        TempData["SuccessMessage"] = "已成功確認活動最終日期！參加成員的該日期已被自動記錄為忙碌。";

        return RedirectToAction(nameof(ActivityDetail), new { id = activityId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelActivity(long activityId)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        await teamService.CancelActivityAsync(userId, activityId);
        TempData["SuccessMessage"] = "已取消該活動，自動解除對成員該日期的占用！";

        return RedirectToAction(nameof(ActivityDetail), new { id = activityId });
    }
}
