using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoilerplateWebApp.Pages;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ILogger<EditModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    public DeleteModel(ILogger<EditModel> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public new IdentityUser User { get; set; } = default!;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

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

        ViewData["authenticatedUserId"] = _userManager.GetUserAsync(HttpContext.User).Result!.Id;

        _logger.LogInformation("");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        StatusMessage = "User has been deleted";

        return Redirect("/UserManagement/");
    }
}