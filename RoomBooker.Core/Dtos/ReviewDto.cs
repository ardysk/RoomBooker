namespace RoomBooker.Core.Dtos
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = default!;
        public int UserId { get; set; }
        public string UserDisplayName { get; set; } = default!;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMyReview { get; set; }
    }
}