using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Entities;
using RoomBooker.Infrastructure.Data;
using RoomBooker.Infrastructure.Services;
using Xunit;

namespace RoomBooker.Tests.Services
{
    public class ReservationServiceTests
    {
        private RoomBookerDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RoomBookerDbContext>()
                .UseInMemoryDatabase(databaseName: $"RoomBooker_{Guid.NewGuid()}")
                .Options;

            return new RoomBookerDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ValidData_CreatesReservationAndAuditLog()
        {
            using var db = CreateContext();

            var user = new User
            {
                DisplayName = "User",
                Email = "user@example.com",
                HashedPassword = "hash",
                Role = "User"
            };
            var room = new Room
            {
                Name = "Sala 101",
                Capacity = 10,
                EquipmentDescription = "Projektor",
                IsActive = true
            };

            db.Users.Add(user);
            db.Rooms.Add(room);
            await db.SaveChangesAsync();

            // Przekazujemy null! jako serwis Google, bo to tylko testy
            var service = new ReservationService(db, null!);

            // ZMIANA: Używamy ReservationCreateDto
            var dto = new ReservationCreateDto
            {
                RoomId = room.RoomId,
                UserId = user.UserId,
                StartTimeUtc = DateTime.UtcNow.AddHours(1),
                EndTimeUtc = DateTime.UtcNow.AddHours(2),
                Purpose = "Spotkanie testowe"
            };

            var created = await service.CreateAsync(dto);

            created.ReservationId.Should().BeGreaterThan(0);
            created.Status.Should().Be("Pending");

            db.Reservations.Count().Should().Be(1);
            db.AuditLogs.Count().Should().Be(1);

            var res = await db.Reservations.SingleAsync();
            res.RoomId.Should().Be(room.RoomId);
            res.UserId.Should().Be(user.UserId);

            var audit = await db.AuditLogs.SingleAsync();
            audit.EntityType.Should().Be("Reservation");
            audit.EntityId.Should().Be(res.ReservationId);
            audit.Action.Should().Be("Create");
        }

        [Fact]
        public async Task CreateAsync_WhenOverlappingReservation_ShouldThrow()
        {
            using var db = CreateContext();

            db.Reservations.Add(new Reservation
            {
                RoomId = 1,
                UserId = 99,
                StartTimeUtc = DateTime.UtcNow.Date.AddHours(10),
                EndTimeUtc = DateTime.UtcNow.Date.AddHours(11),
                Purpose = "Existing",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var service = new ReservationService(db, null!);

            // ZMIANA: ReservationCreateDto
            var dto = new ReservationCreateDto
            {
                RoomId = 1,
                UserId = 2,
                StartTimeUtc = DateTime.UtcNow.Date.AddHours(10.5),
                EndTimeUtc = DateTime.UtcNow.Date.AddHours(11.5),
                Purpose = "Overlap"
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should()
                     .ThrowAsync<InvalidOperationException>()
                     .WithMessage("*zarezerwowana*");
        }

        [Fact]
        public async Task CancelAsync_ChangesStatusToCancelled()
        {
            using var db = CreateContext();

            var reservation = new Reservation
            {
                RoomId = 1,
                UserId = 2,
                StartTimeUtc = DateTime.UtcNow.AddHours(1),
                EndTimeUtc = DateTime.UtcNow.AddHours(2),
                Purpose = "To cancel",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();

            var service = new ReservationService(db, null!);

            var result = await service.CancelAsync(reservation.ReservationId, reservation.UserId);

            result.Should().BeTrue();

            var fromDb = await db.Reservations.SingleAsync(r => r.ReservationId == reservation.ReservationId);
            fromDb.Status.Should().Be("Cancelled");
        }
    }
}