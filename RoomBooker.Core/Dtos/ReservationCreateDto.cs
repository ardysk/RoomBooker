using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class ReservationCreateDto
    {
        [Required(ErrorMessage = "ID pokoju jest wymagane.")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana.")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Data rozpoczęcia musi być w przyszłości.")]
        public DateTime StartTimeUtc { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana.")]
        [DataType(DataType.DateTime)]
        [GreaterThan("StartTimeUtc", ErrorMessage = "Data zakończenia musi być późniejsza niż data rozpoczęcia.")]
        public DateTime EndTimeUtc { get; set; }

        [StringLength(500, ErrorMessage = "Cel rezerwacji nie może mieć więcej niż 500 znaków.")]
        public string? Purpose { get; set; }
    }

    // Niestandardowa walidacja daty w przyszłości
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date && date > DateTime.UtcNow)
                return true;

            return false;
        }
    }

    // Niestandardowa walidacja dla daty zakończenia, aby była późniejsza niż start
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public GreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        public override bool IsValid(object value)
        {
            var currentValue = (DateTime)value;
            var comparisonValue = (DateTime)typeof(ReservationCreateDto)
                .GetProperty(_comparisonProperty)
                .GetValue(value);

            return currentValue > comparisonValue;
        }
    }
}
