using System;

namespace RoomBooker.Core.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}