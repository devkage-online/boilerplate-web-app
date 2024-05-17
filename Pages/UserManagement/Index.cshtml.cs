using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoilerplateWebApp.Pages;

[Authorize(Roles = "Admin")] // Penambahan info protected page sesuai role
public class UserManagementModel : PageModel
{
    private readonly ILogger<UserManagementModel> _logger;

    public UserManagementModel(ILogger<UserManagementModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}