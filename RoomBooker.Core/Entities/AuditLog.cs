using System;

namespace RoomBooker.Core.Entities
{
    public class AuditLog
    {
        public int LogId { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public string EntityType { get; set; } = default!;
        public int? EntityId { get; set; }

        public string Action { get; set; } = default!;

        public string? Details { get; set; }

        public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
    }
}
