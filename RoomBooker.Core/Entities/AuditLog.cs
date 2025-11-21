using System;

namespace RoomBooker.Core.Entities
{
    public class AuditLog
    {
        // KLUCZ GŁÓWNY – MUSI BYĆ LogId
        public int LogId { get; set; }

        // Kto wykonał akcję (może być null np. dla logów systemowych)
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Co było modyfikowane
        public string EntityType { get; set; } = default!; // np. "Reservation"
        public int? EntityId { get; set; }                 // np. ID rezerwacji

        // Jaka akcja: Create / Approve / Reject / Cancel itd.
        public string Action { get; set; } = default!;

        // Szczegóły tekstowe
        public string? Details { get; set; }

        // TO MUSI SIĘ NAZYWAĆ ActionTimestamp
        public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
    }
}
