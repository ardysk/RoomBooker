using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class ReviewCreateDto
    {
        public int RoomId { get; set; }

        [Range(1, 5, ErrorMessage = "Ocena musi być między 1 a 5")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Komentarz max 500 znaków")]
        public string? Comment { get; set; }
    }
}