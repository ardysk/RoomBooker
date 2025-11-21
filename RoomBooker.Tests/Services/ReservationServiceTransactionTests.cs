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
    /// <summary>
    /// Test „transakcyjny” – przy b³êdzie kolizji rezerwacji
    /// nie powstaje ani nowa rezerwacja, ani nowy wpis w AuditLogs.
    /// </summary>
    public class ReservationServiceTransactionTests
    {
        private RoomBookerDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RoomBookerDbContext>()
                .UseInMemoryDatabase(databaseName: $"RoomBooker_Transaction_{Guid.NewGuid()}")
                .Options;

            return new RoomBookerDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_OnOverlapError_DoesNotInsertReservationNorAudit()
        {
            using var db = CreateContext();

            // SEED – istniej¹ca rezerwacja + log
            db.Reservations.Add(new Reservation
            {
                RoomId = 1,
                UserId = 1,
                StartTimeUtc = DateTime.UtcNow.Date.AddHours(10),
                EndTimeUtc = DateTime.UtcNow.Date.AddHours(11),
                Purpose = "Existing",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            db.AuditLogs.Add(new AuditLog
            {
                UserId = 1,
                EntityType = "Reservation",
                EntityId = 1,
                Action = "Create",
                Details = "Seed reservation"
            });

            await db.SaveChangesAsync();

            var initialReservations = db.Reservations.Count();
            var initialAuditLogs = db.AuditLogs.Count();

            var service = new ReservationService(db);

            var dto = new ReservationDto
            {
                RoomId = 1,
                UserId = 2,
                StartTimeUtc = DateTime.UtcNow.Date.AddHours(10.5),
                EndTimeUtc = DateTime.UtcNow.Date.AddHours(11.5),
                Purpose = "Should fail"
            };

            Func<Task> act = async () => await service.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();

            db.Reservations.Count().Should().Be(initialReservations);
            db.AuditLogs.Count().Should().Be(initialAuditLogs);
        }
    }
}
