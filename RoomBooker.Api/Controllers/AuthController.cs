using Microsoft.AspNetCore.Mvc;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Entities;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Data;
using RoomBooker.Infrastructure.Services; // a GoogleAuthService
using Microsoft.EntityFrameworkCore;
// using BCrypt.Net;

namespace RoomBooker.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly RoomBookerDbContext _db;
        private readonly GoogleAuthService _googleService;

        public AuthController(
            IAuthService authService,
            RoomBookerDbContext db,
            GoogleAuthService googleService)
        {
            _authService = authService;
            _db = db;
            _googleService = googleService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (result == null)
                return Unauthorized("Błędny email lub hasło.");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Taki email jest już zajęty.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                HashedPassword = passwordHash,
                Role = "User"
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Rejestracja udana" });
        }

        [HttpGet("google-auth-url")]
        public IActionResult GetGoogleAuthUrl()
        {
            string redirectUrl = "https://localhost:7026/google-callback";

            var url = _googleService.GenerateAuthUrl(redirectUrl);

            return Ok(new { url });
        }

        [HttpPost("google-exchange")]
        public async Task<IActionResult> ConnectGoogle([FromBody] GoogleExchangeDto dto)
        {
            try
            {
                var tokenResponse = await _googleService.ExchangeCodeForTokenAsync(
                    dto.Code,
                    "https://localhost:7026/google-callback"
                );

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null) return NotFound("Użytkownik nie istnieje.");

                user.GoogleAccessToken = tokenResponse.AccessToken;
                user.GoogleRefreshToken = tokenResponse.RefreshToken;
                user.GoogleTokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);

                await _db.SaveChangesAsync();

                return Ok(new { message = "Konto Google połączone!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd Google: {ex.Message}");
            }
        }
        [HttpGet("google-status")]
        public async Task<IActionResult> CheckGoogleStatus([FromQuery] string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            bool isConnected = !string.IsNullOrEmpty(user.GoogleAccessToken);
            return Ok(new { isConnected });
        }
    }
}