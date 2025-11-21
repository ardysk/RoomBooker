using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class RoomDto
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Nazwa pokoju jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa pokoju nie może mieć więcej niż 100 znaków.")]
        public string Name { get; set; } = default!;

        [Range(1, 500, ErrorMessage = "Pojemność pokoju musi wynosić od 1 do 500.")]
        public int Capacity { get; set; }

        [StringLength(500, ErrorMessage = "Opis wyposażenia nie może mieć więcej niż 500 znaków.")]
        public string? EquipmentDescription { get; set; }

        public bool IsActive { get; set; }
    }
}
