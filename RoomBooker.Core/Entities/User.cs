using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooker.Core.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string Email { get; set; } = default!;
        public string HashedPassword { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? GoogleAccessToken { get; set; }
        public string? GoogleRefreshToken { get; set; }
        public DateTime? GoogleTokenExpiration { get; set; }

        // "User"/"Admin"
        public string Role { get; set; } = "User";

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
