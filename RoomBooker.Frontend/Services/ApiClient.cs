using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using RoomBooker.Core.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RoomBooker.Frontend.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _storage;

    private string? _jwtToken;
    public string? JwtToken => _jwtToken;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);

    public ApiClient(HttpClient http, ProtectedLocalStorage storage)
    {
        _http = http;
        _storage = storage;
    }

    private async Task SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(_jwtToken))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
            return;
        }

        try
        {
            var result = await _storage.GetAsync<string>("authToken");
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                _jwtToken = result.Value;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
            }
        }
        catch { }
    }

    public void SetToken(string token)
    {
        _jwtToken = token;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }


    public async Task<bool> LoginAsync(string email, string password)
    {
        var payload = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        var response = await _http.PostAsJsonAsync("/api/Auth/login", payload);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        if (result == null || string.IsNullOrWhiteSpace(result.Token))
            return false;

        SetToken(result.Token);
        return true;
    }

    public Task LogoutAsync()
    {
        _jwtToken = null;
        _http.DefaultRequestHeaders.Authorization = null;
        return Task.CompletedTask;
    }

    public async Task<bool> RegisterAsync(RegisterUserDto dto)
    {
        var response = await _http.PostAsJsonAsync("/api/Auth/register", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> GetGoogleAuthUrlAsync()
    {
        await SetAuthHeader();
        var response = await _http.GetFromJsonAsync<Dictionary<string, string>>("/api/Auth/google-auth-url");
        return response?["url"];
    }

    public async Task<bool> ExchangeGoogleCodeAsync(string code, string email)
    {
        await SetAuthHeader();
        var payload = new { Code = code, Email = email };
        var response = await _http.PostAsJsonAsync("/api/Auth/google-exchange", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IsGoogleConnectedAsync(string email)
    {
        await SetAuthHeader();
        try
        {
            var result = await _http.GetFromJsonAsync<Dictionary<string, bool>>($"/api/Auth/google-status?email={email}");
            return result != null && result["isConnected"];
        }
        catch
        {
            return false;
        }
    }


    public async Task<List<RoomDto>> GetRoomsAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<RoomDto>>("/api/Rooms") ?? new();
    }

    public async Task<RoomDto> CreateRoomAsync(RoomDto room)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/Rooms", room);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RoomDto>())!;
    }

    public async Task<RoomDto?> UpdateRoomAsync(int id, RoomDto room)
    {
        await SetAuthHeader();
        var response = await _http.PutAsJsonAsync($"/api/Rooms/{id}", room);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RoomDto>();
    }

    public async Task DeactivateRoomAsync(int id)
    {
        await SetAuthHeader();
        var response = await _http.PostAsync($"/api/Rooms/{id}/deactivate", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<ReservationDto>> GetReservationsForRoomAsync(int roomId)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<ReservationDto>>($"/api/Reservations/room/{roomId}") ?? new();
    }

    public async Task<ReservationDto> CreateReservationAsync(ReservationCreateDto dto)
    {
        await SetAuthHeader();
        var response = await _http.PostAsJsonAsync("/api/Reservations", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ReservationDto>())!;
    }

    public async Task ApproveReservationAsync(int id)
    {
        await SetAuthHeader();
        (await _http.PostAsync($"/api/Reservations/{id}/approve", null)).EnsureSuccessStatusCode();
    }

    public async Task RejectReservationAsync(int id)
    {
        await SetAuthHeader();
        (await _http.PostAsync($"/api/Reservations/{id}/reject", null)).EnsureSuccessStatusCode();
    }

    public async Task CancelReservationAsync(int id)
    {
        await SetAuthHeader();
        (await _http.PostAsync($"/api/Reservations/{id}/cancel", null)).EnsureSuccessStatusCode();
    }

    public async Task<List<RoomStatDto>> GetRoomStatsAsync(int month, int year)
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<RoomStatDto>>($"/api/Rooms/stats?month={month}&year={year}") ?? new();
    }

    public async Task<Stream> GetReportStreamAsync(int month, int year)
    {
        await SetAuthHeader();
        return await _http.GetStreamAsync($"/api/Rooms/stats/csv?month={month}&year={year}");
    }
    // --- Users (Admin) ---
    public async Task<List<UserDto>> GetUsersAsync()
    {
        await SetAuthHeader();
        return await _http.GetFromJsonAsync<List<UserDto>>("/api/Users") ?? new();
    }

    public async Task DeleteUserAsync(int id)
    {
        await SetAuthHeader();
        var response = await _http.DeleteAsync($"/api/Users/{id}");
        response.EnsureSuccessStatusCode();
    }
}