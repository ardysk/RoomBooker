using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class EquipmentDto
    {
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Musisz przypisać sprzęt do sali")]
        public int RoomId { get; set; }

        public string? RoomName { get; set; }

        public bool IsSelected { get; set; }
    }
}