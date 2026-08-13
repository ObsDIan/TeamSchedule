using System.Text;
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

        try
        {
            var teamName = await teamService.GetTeamNameForMemberAsync(teamId, userId);
            var model = new CreateActivityViewModel
            {
                TeamId = teamId,
                TeamName = teamName
            };

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
    public async Task<IActionResult> CreateActivity(CreateActivityViewModel model)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        if (string.IsNullOrWhiteSpace(model.CandidateDatesInput))
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), "請至少輸入一個候選日期。");
        }

        if (!ModelState.IsValid) return View(model);

        var dates = new List<DateTime>();
        var parts = model.CandidateDatesInput.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (DateTime.TryParse(p.Trim(), out var parsedDate))
            {
                dates.Add(parsedDate);
            }
        }

        if (dates.Count == 0)
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), "無法解析日期格式，請使用 YYYY-MM-DD 格式。");
            return View(model);
        }

        var distinctDates = dates.Select(d => d.Date).Distinct().ToList();
        if (distinctDates.Count > 30)
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), "候選日期最多 30 個，請精簡選擇。");
            return View(model);
        }

        if (distinctDates.Any(d => d < DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), "候選日期不能是過去的日期（今天與未來日期均可），請重新選擇。");
            return View(model);
        }

        try
        {
            var activityId = await teamService.CreateActivityAsync(userId, model.TeamId, model.Title, model.Description, dates);
            TempData["SuccessMessage"] = "已成功建立團隊活動！";

            return RedirectToAction(nameof(ActivityDetail), new { id = activityId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.CandidateDatesInput), ex.Message);
            return View(model);
        }
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

        try
        {
            await teamService.ConfirmActivityAsync(userId, activityId, candidateDateId);
            TempData["SuccessMessage"] = "已成功確認活動最終日期！參加成員的該日期已被自動記錄為忙碌。";

            return RedirectToAction(nameof(ActivityDetail), new { id = activityId });
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
    public async Task<IActionResult> CancelActivity(long activityId)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            await teamService.CancelActivityAsync(userId, activityId);
            TempData["SuccessMessage"] = "已取消該活動，自動解除對成員該日期的占用！";

            return RedirectToAction(nameof(ActivityDetail), new { id = activityId });
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

    public async Task<IActionResult> ExportIcs(long activityId)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            var activity = await teamService.GetActivityForExportAsync(activityId, userId);
            var teamName = activity.Team?.TeamName ?? "";

            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCALENDAR");
            builder.AppendLine("VERSION:2.0");
            builder.AppendLine("PRODID:-//TeamSchedule//TeamSchedule//ZH-TW");
            builder.AppendLine("CALSCALE:GREGORIAN");
            builder.AppendLine("METHOD:PUBLISH");
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:{activity.ActivityId}@teamschedule.local");
            builder.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            builder.AppendLine($"DTSTART;VALUE=DATE:{activity.FinalDate!.Value:yyyyMMdd}");
            builder.AppendLine($"SUMMARY:{EscapeIcsText(activity.Title)}");
            var description = string.IsNullOrWhiteSpace(activity.Description)
                ? $"團隊：{teamName}"
                : $"{activity.Description}\n團隊：{teamName}";
            builder.AppendLine($"DESCRIPTION:{EscapeIcsText(description)}");
            builder.AppendLine("END:VEVENT");
            builder.AppendLine("END:VCALENDAR");

            var fileName = $"{SanitizeFileName(activity.Title)}.ics";
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/calendar", fileName);
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

    private static string EscapeIcsText(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
    }

    private static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrEmpty(cleaned) ? "活動" : cleaned;
    }
}
