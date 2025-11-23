using FluentValidation.TestHelper;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Validators;
using Xunit;

namespace RoomBooker.Tests
{
    public class ValidatorTests
    {
        private readonly ReservationValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_Purpose_Is_Empty()
        {
            var model = new ReservationCreateDto { Purpose = "" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Purpose);
        }

        [Fact]
        public void Should_Have_Error_When_Date_Is_In_Past()
        {
            var model = new ReservationCreateDto
            {
                StartTimeUtc = DateTime.UtcNow.AddDays(-1)
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.StartTimeUtc);
        }

        [Fact]
        public void Should_Pass_When_Data_Is_Correct()
        {
            var model = new ReservationCreateDto
            {
                Purpose = "Valid Meeting",
                StartTimeUtc = DateTime.UtcNow.AddDays(1),
                EndTimeUtc = DateTime.UtcNow.AddDays(1).AddHours(2),
                RoomId = 1
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}