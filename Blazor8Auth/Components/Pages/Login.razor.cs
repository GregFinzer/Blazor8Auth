using System.Security.Claims;
using Blazor8Auth.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;

namespace Blazor8Auth.Components.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private AuthService AuthService { get; set; }
        [Inject] private AuthDataService AuthDataService { get; set; }

        protected SignInModel loginModel = new();

        protected string DisplayError { get; set; } = "none;";
        public bool showPassword { get; set; }
        public string? passwordType { get; set; }
        public string errorMessage { get; set; }
        protected InputText? inputTextFocus;
        string returnUrl;

        protected override void OnInitialized()
        {
            passwordType = "password";
            showPassword = false;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (String.IsNullOrEmpty(returnUrl))
            {
                var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
                if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var url))
                {
                    returnUrl = url;
                }
            }

            if (firstRender && !AuthService.IsLoggedIn)
            {
                var restoredFromState = await AuthService.GetStateFromTokenAsync();
                if (restoredFromState)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        NavigationManager.NavigateTo(returnUrl);
                    }
                }
            }
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
            var loginResult = AuthDataService.Login(loginModel.Email, loginModel.Password);

            if (!loginResult.Success)
            {
                errorMessage = loginResult.Message;
                DisplayError = "block;";

                if (inputTextFocus.Element != null)
                {
                    await inputTextFocus.Element.Value.FocusAsync();
                }
            }
            else 
            {
                await AuthService.Login(loginResult.Data);

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    NavigationManager.NavigateTo(returnUrl);
                    returnUrl = string.Empty;
                }
                else
                {
                    NavigationManager.NavigateTo("/auth");
                }
            }

        }
    }
}
