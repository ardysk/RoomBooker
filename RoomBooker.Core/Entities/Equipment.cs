namespace RoomBooker.Core.Entities
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        public string Name { get; set; } = default!;

        public int RoomId { get; set; }
        public Room Room { get; set; } = default!;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}