using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoilerplateWebApp.Pages;

[Authorize(Roles = "Admin")] // Penambahan info protected page sesuai role
public class UserManagementModel : PageModel
{
    private readonly ILogger<UserManagementModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public UserManagementModel(ILogger<UserManagementModel> logger, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public new IList<IdentityUser> User { get; set; } = default!;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public void OnGet()
    {
        User = _userManager.Users.ToList();
        _logger.LogInformation("");
    }
}