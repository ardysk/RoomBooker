using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooker.Core.Entities
{
    public class AuditLog
    {
        public int LogId { get; set; }

        // Kto wykonał akcję (może być null np. dla błędów systemowych)
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Np. "CreateReservation", "ApproveReservation", "LoginFailed", "Error500"
        public string ActionType { get; set; } = default!;

        public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;

        // Szczegóły (np. ReservationId, stary/nowy status)
        public string? Details { get; set; }
    }
}
