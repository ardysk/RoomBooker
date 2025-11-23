using System.ComponentModel.DataAnnotations;

namespace RoomBooker.Core.Dtos;

public class ReservationCreateDto : IValidatableObject
{
    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTime StartTimeUtc { get; set; }

    [Required]
    public DateTime EndTimeUtc { get; set; }

    [Required]
    [StringLength(200)]
    public string Purpose { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTimeUtc <= StartTimeUtc)
        {
            yield return new ValidationResult(
                "Czas zakończenia musi być późniejszy niż czas rozpoczęcia.",
                new[] { nameof(EndTimeUtc) });
        }
    }
}
