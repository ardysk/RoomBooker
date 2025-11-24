using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooker.Core.Entities
{
    public class Room
    {
        public int RoomId { get; set; }

        public string Name { get; set; } = default!;
        public int Capacity { get; set; }

        public string? EquipmentDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<MaintenanceWindow> MaintenanceWindows { get; set; } = new List<MaintenanceWindow>();
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
