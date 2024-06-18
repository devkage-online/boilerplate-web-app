using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoilerplateWebApp.Pages;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ILogger<EditModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    public EditModel(ILogger<EditModel> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public new IdentityUser User { get; set; } = default!;
    public string Role { get; set; } = string.Empty;
    [BindProperty]
    public string SelectedRole { get; set; } = string.Empty;
    [TempData]
    public string StatusMessage { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string> { "Admin", "User" };

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        else
        {
            User = user;
        }

        var roles = await _userManager.GetRolesAsync(user);

        Role = roles.FirstOrDefault()!;

        _logger.LogInformation("");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(id!);

        if (user == null)
        {
            return NotFound();
        }
        else
        {
            User = user;
        }

        var userRoles = await _userManager.GetRolesAsync(User);
        var currentRole = userRoles.FirstOrDefault()!;

        if (currentRole != SelectedRole)
        {
            if (!string.IsNullOrEmpty(currentRole))
            {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            }
            if (!string.IsNullOrEmpty(SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, SelectedRole);
            }

            await _userManager.UpdateSecurityStampAsync(user);

            var authenticatedUserId = _userManager.GetUserAsync(HttpContext.User).Result!.Id; // User yang sedang login
            if (id == authenticatedUserId)
            {
                await _signInManager.SignOutAsync();
            }
        }

        StatusMessage = "User role has been updated";

        return Redirect($"/UserManagement/Edit?id={id}");
    }
}