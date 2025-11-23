using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }

        public int Id
        {
            get => ReservationId;
            set => ReservationId = value;
        }

        [Required(ErrorMessage = "ID pokoju jest wymagane.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "ID użytkownika jest wymagane.")]
        public int UserId { get; set; }

        public int? ApprovedBy { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana.")]
        public DateTime StartTimeUtc { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana.")]
        public DateTime EndTimeUtc { get; set; }

        [Required(ErrorMessage = "Cel rezerwacji jest wymagany.")]
        [StringLength(1000, ErrorMessage = "Cel rezerwacji nie może mieć więcej niż 1000 znaków.")]
        public string Purpose { get; set; } = default!;

        public string Status { get; set; } = "Pending";

        public string? RejectionReason { get; set; }

        public string? RoomName { get; set; }

    }
}
