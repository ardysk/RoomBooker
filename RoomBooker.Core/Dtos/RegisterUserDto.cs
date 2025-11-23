using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format emaila.")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        public string Password { get; set; } = default!;

        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        public string DisplayName { get; set; } = default!;
    }
}
