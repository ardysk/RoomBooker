using Bogus;
using RoomBooker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace RoomBooker.Infrastructure.Data
{
    public class DataSeeder
    {
        private readonly RoomBookerDbContext _db;

        public DataSeeder(RoomBookerDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            if (await _db.Reservations.AnyAsync()) return;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Role == "User");
            var rooms = await _db.Rooms.ToListAsync();

            if (user == null || !rooms.Any()) return;

            var reservations = new List<Reservation>();
            var random = new Random();

            for (int i = 0; i < 100; i++)
            {
                var room = rooms[random.Next(rooms.Count)];

                var start = DateTime.UtcNow.AddDays(random.Next(1, 30)).AddHours(random.Next(8, 16));
                var end = start.AddHours(random.Next(1, 4));

                reservations.Add(new Reservation
                {
                    RoomId = room.RoomId,
                    UserId = user.UserId,
                    StartTimeUtc = start,
                    EndTimeUtc = end,
                    Purpose = $"Automatyczna rezerwacja nr {i}",
                    Status = i % 5 == 0 ? "Rejected" : (i % 3 == 0 ? "Approved" : "Pending"),
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.Reservations.AddRangeAsync(reservations);
            await _db.SaveChangesAsync();
        }
    }
}