using System;

namespace RoomBooker.Core.Entities
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        // Relacje
        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int? ApprovedBy { get; set; }
        public User? ApprovedByUser { get; set; }

        // CZAS REZERWACJI – TO BYŁO POTRZEBNE W SERWISIE
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        // OPIS CELU
        public string Purpose { get; set; } = default!;

        // Status: Pending / Approved / Rejected / Cancelled
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RejectionReason { get; set; }
    }
}
