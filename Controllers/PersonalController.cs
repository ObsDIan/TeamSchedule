using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeamSchedule.Models;
using TeamSchedule.Services;
using TeamSchedule.ViewModels;

namespace TeamSchedule.Controllers;

[Authorize]
public class PersonalController(
    IAvailabilityService availabilityService,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Calendar(int? year, int? month)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var now = DateTime.Today;
        int targetYear = year ?? now.Year;
        int targetMonth = month ?? now.Month;

        var model = await availabilityService.GetPersonalCalendarMonthAsync(userId, targetYear, targetMonth);
        return View(model);
    }

    public async Task<IActionResult> WeeklySetup()
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var model = new WeeklySetupViewModel();

        // Load saved weekly settings directly from DB
        var savedSettings = await availabilityService.GetWeeklyAvailabilityAsync(userId);

        // Default rule for unset days: Mon-Fri Busy, Sat-Sun Available
        for (int day = 0; day <= 6; day++)
        {
            if (savedSettings.TryGetValue(day, out var savedStatus))
            {
                model.DaySettings[day] = savedStatus;
            }
            else
            {
                model.DaySettings[day] = day == 0 || day == 6 ? AvailabilityStatus.Available : AvailabilityStatus.Busy;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WeeklySetup(WeeklySetupViewModel model)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        if (model.DaySettings.Count != 7 ||
            model.DaySettings.Keys.Any(day => day is < 0 or > 6) ||
            model.DaySettings.Values.Any(status => !Enum.IsDefined(status)))
        {
            ModelState.AddModelError(nameof(model.DaySettings), "每週設定資料不完整或格式不正確。");
            return View(model);
        }

        await availabilityService.SaveWeeklyAvailabilityAsync(userId, model.DaySettings);
        TempData["SuccessMessage"] = "已成功更新您的每週預設狀態！";

        return RedirectToAction(nameof(WeeklySetup));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetOverride([FromBody] SetDateOverrideRequestModel? request)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { success = false, message = "登入狀態已失效，請重新登入。" });

        if (request == null || request.Date == default)
        {
            return BadRequest(new { success = false, message = "日期資料不完整。" });
        }

        if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
        {
            return BadRequest(new { success = false, message = "狀態格式不正確。" });
        }

        var note = request.Note?.Trim();
        if (note?.Length > 500)
        {
            return BadRequest(new { success = false, message = "備註不能超過 500 個字。" });
        }

        await availabilityService.SetDateOverrideAsync(userId, request.Date, request.Status, note);
        return Json(new { success = true });
    }
}
