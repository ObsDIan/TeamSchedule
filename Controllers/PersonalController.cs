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

        // Load existing weekly settings or default (Mon-Fri: Busy, Sat-Sun: Available)
        for (int day = 0; day <= 6; day++)
        {
            var status = await availabilityService.GetFinalStatusAsync(userId, DateTime.Today.AddDays(day - (int)DateTime.Today.DayOfWeek));
            // Default rule if not set
            if (day == 0 || day == 6)
            {
                model.DaySettings[day] = AvailabilityStatus.Available;
            }
            else
            {
                model.DaySettings[day] = AvailabilityStatus.Busy;
            }
        }

        // Fetch actual saved values from DB
        var savedMonth = await availabilityService.GetPersonalCalendarMonthAsync(userId, DateTime.Today.Year, DateTime.Today.Month);
        foreach (var dayModel in savedMonth.Days)
        {
            int dayOfWeek = (int)dayModel.Date.DayOfWeek;
            if (dayModel.Status.HasValue && !dayModel.IsConfirmedActivityBusy)
            {
                model.DaySettings[dayOfWeek] = dayModel.Status.Value;
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
    public async Task<IActionResult> SetOverride([FromBody] SetDateOverrideRequestModel request)
    {
        var userId = userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "未登入" });

        await availabilityService.SetDateOverrideAsync(userId, request.Date, request.Status, request.Note);
        return Json(new { success = true });
    }
}
