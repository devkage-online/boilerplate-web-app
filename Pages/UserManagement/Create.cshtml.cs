using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BoilerplateWebApp.Pages;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ILogger<EditModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IUserStore<IdentityUser> _userStore;
    private readonly IUserEmailStore<IdentityUser> _emailStore;
    private readonly IEmailSender _emailSender;

    public CreateModel(
        ILogger<EditModel> logger, 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager,
        IUserStore<IdentityUser> userStore,
        IEmailSender emailSender)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _emailSender = emailSender;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = String.Empty;
    public string Password { get; set; } = "DevK@ge0nline";
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public IActionResult OnGetAsync()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new IdentityUser
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, Password); // Membuat user dengan default password

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account without password.");
            await _userManager.AddToRoleAsync(user, "User");
            await _emailSender.SendEmailAsync(Email, "Confirm your email", $"Your default password is <b>{Password}</b>");
            StatusMessage = "User has been created successfully.";
            return RedirectToPage("/UserManagement/Index");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }

    private IUserEmailStore<IdentityUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<IdentityUser>)_userStore;
    }
}