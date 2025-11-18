using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooker.Core.Entities
{
    public class MaintenanceWindow
    {
        public int BlockId { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Np. "Sprzątanie", "Awaria projektora"
        public string Reason { get; set; } = default!;
    }
}
