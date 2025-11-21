using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format emaila.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Rola użytkownika jest wymagana.")]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Rola musi być 'Admin' lub 'User'.")]
        public string Role { get; set; } = default!;
    }
}
