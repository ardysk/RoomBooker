using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace RoomBooker.Frontend.Services
{
    public class AuthState : AuthenticationStateProvider
    {
        private readonly ApiClient _apiClient;
        private readonly ProtectedLocalStorage _storage;

        private string? _userEmail;
        private bool _successfullyLoaded = false;

        public AuthState(ApiClient apiClient, ProtectedLocalStorage storage)
        {
            _apiClient = apiClient;
            _storage = storage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var emptyState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var tokenResult = await _storage.GetAsync<string>("authToken");

                var emailResult = await _storage.GetAsync<string>("userEmail");

                if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
                {
                    return emptyState;
                }

                var token = tokenResult.Value;
                _apiClient.SetToken(token);

                if (emailResult.Success) _userEmail = emailResult.Value;

                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");

                if (string.IsNullOrEmpty(_userEmail))
                {
                    _userEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                                 ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;
                }

                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                return emptyState;
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var ok = await _apiClient.LoginAsync(email, password);
            if (ok)
            {
                _userEmail = email;
                if (!string.IsNullOrEmpty(_apiClient.JwtToken))
                {
                    await _storage.SetAsync("authToken", _apiClient.JwtToken);
                    await _storage.SetAsync("userEmail", email);
                }
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            return ok;
        }

        public async Task LogoutAsync()
        {
            await _apiClient.LogoutAsync();
            _userEmail = null;
            await _storage.DeleteAsync("authToken");
            await _storage.DeleteAsync("userEmail");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null) return claims;

            foreach (var kvp in keyValuePairs)
            {
                var claimType = kvp.Key;
                if (claimType == "role" || claimType == "Role") claimType = ClaimTypes.Role;
                else if (claimType == "unique_name") claimType = ClaimTypes.Name;
                else if (claimType == "email") claimType = ClaimTypes.Email;

                if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                        claims.Add(new Claim(claimType, item.ToString()));
                }
                else
                {
                    claims.Add(new Claim(claimType, kvp.Value.ToString()!));
                }
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
        public string? UserEmail => _userEmail;

        public bool IsAuthenticated => _apiClient.IsAuthenticated;
    }
}