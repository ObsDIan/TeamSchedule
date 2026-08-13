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

        await availabilityService.SaveWeeklyAvailabilityAsync(userId, model.DaySettings);
        TempData["SuccessMessage"] = "已成功更新您的每週預設狀態！";

        return RedirectToAction(nameof(WeeklySetup));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetOverride([FromBody] SetDateOverrideRequestModel request)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "未登入" });

        await availabilityService.SetDateOverrideAsync(userId, request.Date, request.Status, request.Note);
        return Json(new { success = true });
    }
}
