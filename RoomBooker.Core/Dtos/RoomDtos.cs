using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomBooker.Core.Dtos
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string Name { get; set; } = default!;
        public int Capacity { get; set; }
        public string? EquipmentDescription { get; set; }
        public bool IsActive { get; set; }
    }
}

