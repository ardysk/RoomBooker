using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class ReviewDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }
    }
}