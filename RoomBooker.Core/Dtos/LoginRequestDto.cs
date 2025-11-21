using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format emaila.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        public required string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
