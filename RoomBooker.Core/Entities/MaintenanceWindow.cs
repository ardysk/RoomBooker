using System;

namespace RoomBooker.Core.Entities
{
    public class MaintenanceWindow
    {
        public int BlockId { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
