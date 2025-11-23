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

                if (!tokenResult.Success || string.IsNullOrEmpty(tokenResult.Value))
                {
                    return emptyState;
                }

                var token = tokenResult.Value;

                _apiClient.SetToken(token);

                var claims = ParseClaimsFromJwt(token);

                var identity = new ClaimsIdentity(claims, "jwt");
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
                if (claimType == "role" || claimType == "Role")
                {
                    claimType = ClaimTypes.Role;
                }
                else if (claimType == "unique_name")
                {
                    claimType = ClaimTypes.Name;
                }

                if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(claimType, item.ToString()));
                    }
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

        public string? UserEmail => _apiClient.IsAuthenticated ? "Zalogowany" : null;
        public bool IsAuthenticated => _apiClient.IsAuthenticated;
    }
}