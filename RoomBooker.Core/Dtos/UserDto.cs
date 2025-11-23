using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format emaila.")]
        public required string Email { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? Password { get; set; }

        [Required(ErrorMessage = "Rola użytkownika jest wymagana.")]
        public string Role { get; set; } = default!;
    }
}