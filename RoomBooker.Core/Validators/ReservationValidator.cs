using FluentValidation;
using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Validators
{
    public class ReservationValidator : AbstractValidator<ReservationCreateDto>
    {
        public ReservationValidator()
        {
            RuleFor(x => x.Purpose)
                .NotEmpty().WithMessage("Cel jest wymagany.")
                .Length(5, 200).WithMessage("Cel musi mieć od 5 do 200 znaków.");

            RuleFor(x => x.StartTimeUtc)
                .GreaterThan(DateTime.UtcNow).WithMessage("Rezerwacja musi być w przyszłości.");

            RuleFor(x => x.EndTimeUtc)
                .GreaterThan(x => x.StartTimeUtc).WithMessage("Koniec musi być po początku.")
                .Must((dto, end) => (end - dto.StartTimeUtc).TotalHours <= 8)
                .WithMessage("Rezerwacja nie może być dłuższa niż 8 godzin.");
        }
    }
}