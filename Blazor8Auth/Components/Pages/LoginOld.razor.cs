using System.Security.Claims;
using Blazor8Auth.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;

namespace Blazor8Auth.Components.Pages
{
    public partial class LoginOld : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private AuthService AuthService { get; set; }

        protected SignInModel loginModel = new();

        protected string DisplayError { get; set; } = "none;";
        protected string Error = "";
        public bool showPassword { get; set; }
        public string? passwordType { get; set; }
        public string errorMessage { get; set; }
        protected InputText? inputTextFocus;

        protected override void OnInitialized()
        {
            passwordType = "password";
        }

        protected void HandlePassword()
        {
            if (showPassword)
            {
                passwordType = "password";
                showPassword = false;
            }
            else
            {
                passwordType = "text";
                showPassword = true;
            }
        }

        protected async Task HandleLogin()
        {
            if (string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                Error = "Please enter your email and password";
                DisplayError = "block;";

                if (inputTextFocus.Element != null)
                {
                    await inputTextFocus.Element.Value.FocusAsync();
                }
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginModel.Email),
                    new Claim(ClaimTypes.Email, loginModel.Email),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, "jwt");
                var principal = new ClaimsPrincipal(identity);
                await AuthService.Login(principal);
                NavigationManager.NavigateTo("/auth");
            }

        }
    }
}
