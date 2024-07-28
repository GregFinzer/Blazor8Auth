using Blazor8Auth.Entities;
using System.Security.Claims;

namespace Blazor8Auth.Services
{
    public interface IAuthDataService
    {
        ServiceResponse<ClaimsPrincipal> Login(string email, string password);
    }
}
