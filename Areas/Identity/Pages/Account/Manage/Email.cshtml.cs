using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeamSchedule.Models;

namespace TeamSchedule.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public EmailModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Email { get; set; } = string.Empty;

    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "請輸入新的 Email 電子郵件")]
        [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
        [Display(Name = "新 Email 電子郵件")]
        public string NewEmail { get; set; } = string.Empty;
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        var email = await _userManager.GetEmailAsync(user);
        Email = email ?? string.Empty;
        Input = new InputModel { NewEmail = email ?? string.Empty };
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"無法載入使用者 ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"無法載入使用者 ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.NewEmail != email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, Input.NewEmail);
            var setUsernameResult = await _userManager.SetUserNameAsync(user, Input.NewEmail);

            if (!setEmailResult.Succeeded || !setUsernameResult.Succeeded)
            {
                StatusMessage = "更新 Email 失敗，該 Email 可能已被其他帳號使用。";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "✅ Email 電子郵件與登入帳號已成功更新！";
            return RedirectToPage();
        }

        StatusMessage = "您的 Email 未做任何變更。";
        return RedirectToPage();
    }
}
