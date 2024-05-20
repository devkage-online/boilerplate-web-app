using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoilerplateWebApp.Pages;

[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly ILogger<DetailsModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public DetailsModel(ILogger<DetailsModel> logger, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public new IdentityUser User { get; set; } = default!;

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
        return Page();
    }
}