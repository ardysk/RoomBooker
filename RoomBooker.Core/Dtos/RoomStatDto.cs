namespace RoomBooker.Core.Dtos
{
    public class RoomStatDto
    {
        public string RoomName { get; set; } = default!;
        public int ReservationCount { get; set; }
        public int TotalHours { get; set; }
    }
}