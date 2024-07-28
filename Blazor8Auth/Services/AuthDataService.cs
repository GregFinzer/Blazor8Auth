using System.Security.Claims;
using Blazor8Auth.Entities;

namespace Blazor8Auth.Services
{
    /// <summary>
    /// This is an example of a data service that would be used to authenticate a user.
    /// </summary>
    public class AuthDataService : IAuthDataService
    {
        public ServiceResponse<ClaimsPrincipal> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new ServiceResponse<ClaimsPrincipal>("Email is required");
            }

            if (string.IsNullOrEmpty(password))
            {
                return new ServiceResponse<ClaimsPrincipal>("Password is required");
            }

            if (email != "super@acme.com" && email != "admin@acme.com")
            {
                return new ServiceResponse<ClaimsPrincipal>(
                    "Unknown user, please enter super@acme.com or admin@acme.com");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, email.StartsWith("admin") ? "Admin" : "Super")
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            return new ServiceResponse<ClaimsPrincipal>("Login successful", true, principal);
        }
    }
}
