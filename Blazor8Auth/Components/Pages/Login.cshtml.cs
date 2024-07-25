using System.Security.Claims;
using Blazor8Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blazor8Auth.Components.Pages
{
    public class LoginModel : PageModel
    {
        [CascadingParameter]
        public HttpContext HttpContext { get; set; }

        [Inject]
        public AuthService AuthService { get; set; }

        [BindProperty]
        public SignInModel Model { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Model.Email),
                new Claim(ClaimTypes.Email, Model.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);
            await AuthService.Login(principal);
            return RedirectToPage("/");
        }
    }
}
