using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public RegisterModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } 

    public class InputModel
    {
        public required string Email { get; set; } = "";
        public required string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new IdentityUser
        {
            UserName = Input.Email,
            Email = Input.Email
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
            return RedirectToPage("Login");

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return Page();
    }
}