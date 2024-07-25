using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Blazor8Auth.Services
{
    public class AuthService
    {
        const string AuthTokenName = "auth_token";
        public event Action<ClaimsPrincipal>? UserChanged;
        private ClaimsPrincipal? currentUser;
        private readonly ICustomSessionService _sessionService;
        private readonly IConfiguration _configuration;

        public AuthService(ICustomSessionService sessionService, IConfiguration configuration)
        {
            _sessionService = sessionService;
            _configuration = configuration;
        }

        public ClaimsPrincipal CurrentUser
        {
            get { return currentUser ?? new(); }
            set
            {
                currentUser = value;

                if (UserChanged is not null)
                {
                    UserChanged(currentUser);
                }
            }
        }

        public bool IsLoggedIn => CurrentUser.Identity?.IsAuthenticated ?? false;

        public async Task LogoutAsync()
        {
            CurrentUser = new();
            string authToken = await _sessionService.GetItemAsStringAsync(AuthTokenName);

            if (!string.IsNullOrEmpty(authToken))
            {
                await _sessionService.RemoveItemAsync(AuthTokenName);
            }
        }

        public async Task GetStateFromTokenAsync()
        {
            string authToken = await _sessionService.GetItemAsStringAsync(AuthTokenName);

            var identity = new ClaimsIdentity();

            if (!string.IsNullOrEmpty(authToken))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value);

                    tokenHandler.ValidateToken(authToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                }
                catch
                {
                    await _sessionService.RemoveItemAsync(AuthTokenName);
                    identity = new ClaimsIdentity();
                }
            }

            var user = new ClaimsPrincipal(identity);
            CurrentUser = user;
        }


        public async Task Login(ClaimsPrincipal user)
        {
            CurrentUser = user;

            var tokenEncryptionKey = _configuration.GetSection("AppSettings:Token").Value;
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(tokenEncryptionKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenHoursString = _configuration.GetSection("AppSettings:TokenHours").Value;
            int.TryParse(tokenHoursString, out int tokenHours);
            var token = new JwtSecurityToken(
                claims: user.Claims,
                expires: DateTime.Now.AddHours(tokenHours),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            await _sessionService.SetItemAsStringAsync(AuthTokenName, jwt);
        }
    }
}
