using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos
{
    public class ReservationCreateDto : IValidatableObject
    {
        public int? RoomId { get; set; }
        public int UserId { get; set; }
        [Required]
        public DateTime StartTimeUtc { get; set; }

        [Required]
        public DateTime EndTimeUtc { get; set; }

        [Required]
        [StringLength(200)]
        public string Purpose { get; set; } = string.Empty;

        public List<int> SelectedEquipmentIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1. Sprawdź czy koniec jest po początku
            if (EndTimeUtc <= StartTimeUtc)
            {
                yield return new ValidationResult(
                    "Czas zakończenia musi być późniejszy niż czas rozpoczęcia.",
                    new[] { nameof(EndTimeUtc) });
            }

            // 2. NOWOŚĆ: Sprawdź czy start jest w przyszłości (+1 minuta marginesu)
            if (StartTimeUtc < DateTime.UtcNow.AddMinutes(1))
            {
                yield return new ValidationResult(
                   "Rezerwacja musi rozpoczynać się w przyszłości (minimum za minutę).",
                   new[] { nameof(StartTimeUtc) });
            }

            // 3. Sprawdź czy wybrano zasób
            bool hasRoom = RoomId.HasValue && RoomId.Value > 0;
            bool hasEquipment = SelectedEquipmentIds != null && SelectedEquipmentIds.Any();

            if (!hasRoom && !hasEquipment)
            {
                yield return new ValidationResult(
                    "Musisz wybrać Salę LUB co najmniej jeden przedmiot do wypożyczenia.",
                    new[] { nameof(RoomId) });
            }
        }
    }
}