using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Niepoprawny format emaila.")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        public string DisplayName { get; set; } = default!;

        [Required(ErrorMessage = "Rola jest wymagana.")]
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Rola musi być 'Admin' lub 'User'.")]
        public string Role { get; set; } = default!;

        //[MinLength(6, ErrorMessage = "Hasło musi mieć min. 6 znaków.")]
        public string? NewPassword { get; set; }
    }
}