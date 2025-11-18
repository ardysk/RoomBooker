using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooker.Core.Entities
{
    public class Reservation
    {
        public int ReservationId { get; set; }

        // FK do sali
        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        // FK do użytkownika, który złożył rezerwację
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Status rezerwacji:
        /// "Pending", "Approved", "Rejected", "Canceled"
        /// </summary>
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Admin, który zatwierdził rezerwację (jeśli dotyczy)
        public int? ApprovedBy { get; set; }
        public User? ApprovedByUser { get; set; }

        public DateTime? ApprovedAt { get; set; }
    }
}
