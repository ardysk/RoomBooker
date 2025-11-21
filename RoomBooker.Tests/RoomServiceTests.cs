using System;
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
    public class RoomServiceTests
    {
        private RoomBookerDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RoomBookerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RoomBookerDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_CreatesRoom()
        {
            using var db = CreateContext();
            var service = new RoomService(db);

            var dto = new RoomDto
            {
                Name = "New room",
                Capacity = 10,
                EquipmentDescription = "Projector",
                IsActive = true
            };

            var created = await service.CreateAsync(dto);

            created.RoomId.Should().BeGreaterThan(0);
            created.Name.Should().Be(dto.Name);

            var fromDb = await db.Rooms.SingleAsync();
            fromDb.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task DeactivateAsync_SetsIsActiveFalse()
        {
            using var db = CreateContext();

            var room = new Room
            {
                Name = "To deactivate",
                Capacity = 5,
                EquipmentDescription = null,
                IsActive = true
            };
            db.Rooms.Add(room);
            await db.SaveChangesAsync();

            var service = new RoomService(db);

            var result = await service.DeactivateAsync(room.RoomId);

            result.Should().BeTrue();

            var fromDb = await db.Rooms.SingleAsync(r => r.RoomId == room.RoomId);
            fromDb.IsActive.Should().BeFalse();
        }
    }
}
