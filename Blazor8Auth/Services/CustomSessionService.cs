using Blazored.SessionStorage;

namespace Blazor8Auth.Services
{
    /// <summary>
    /// A service to manage the browser session storage using Blazored.SessionStorage
    /// </summary>
    public class CustomSessionService : ICustomSessionService
    {
        private readonly ISessionStorageService _sessionService;

        public CustomSessionService(ISessionStorageService sessionService)
        {
            _sessionService = sessionService;
        }

        public async Task<string> GetItemAsStringAsync(string key)
        {
            return await _sessionService.GetItemAsStringAsync(key);
        }

        public async Task SetItemAsStringAsync(string key, string value)
        {
            await _sessionService.SetItemAsStringAsync(key, value);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _sessionService.RemoveItemAsync(key);
        }
    }
}
