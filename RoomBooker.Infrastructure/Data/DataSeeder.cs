using Bogus;
using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Entities;
using RoomBooker.Infrastructure.Data;

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
            if (await _db.Reservations.CountAsync() > 20) return;

            Randomizer.Seed = new Random(123456);
            var users = await _db.Users.ToListAsync();
            if (users.Count < 5)
            {
                var passwordHash = "$2a$11$7S.zV5.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z.z";

                var userFaker = new Faker<User>()
                    .RuleFor(u => u.Email, f => f.Internet.Email())
                    .RuleFor(u => u.DisplayName, f => f.Name.FullName())
                    .RuleFor(u => u.HashedPassword, f => passwordHash)
                    .RuleFor(u => u.Role, f => "User");

                var newUsers = userFaker.Generate(50);
                await _db.Users.AddRangeAsync(newUsers);
                await _db.SaveChangesAsync();
                users = await _db.Users.ToListAsync();
            }

            var rooms = await _db.Rooms.ToListAsync();
            if (rooms.Count < 5)
            {
                var roomFaker = new Faker<Room>()
                    .RuleFor(r => r.Name, f => $"Sala {f.Commerce.Color()} {f.Random.Number(100, 999)}")
                    .RuleFor(r => r.Capacity, f => f.Random.Number(4, 50))
                    .RuleFor(r => r.EquipmentDescription, f => f.Lorem.Sentence())
                    .RuleFor(r => r.IsActive, f => true);

                var newRooms = roomFaker.Generate(15);
                await _db.Rooms.AddRangeAsync(newRooms);
                await _db.SaveChangesAsync();
                rooms = await _db.Rooms.ToListAsync();
            }

            if (!await _db.Equipments.AnyAsync())
            {
                var equipmentFaker = new Faker<Equipment>()
                    .RuleFor(e => e.Name, f => f.Commerce.ProductName());

                var allEquipments = new List<Equipment>();
                var rnd = new Random(123456);

                foreach (var room in rooms)
                {
                    int itemsCount = rnd.Next(3, 8);

                    var roomEquipments = equipmentFaker.Clone()
                        .RuleFor(e => e.RoomId, f => room.RoomId)
                        .Generate(itemsCount);

                    allEquipments.AddRange(roomEquipments);
                }

                await _db.Equipments.AddRangeAsync(allEquipments);
                await _db.SaveChangesAsync();
            }

            var allDbEquipment = await _db.Equipments.ToListAsync();

            var reservationFaker = new Faker<Reservation>()
                .RuleFor(r => r.RoomId, f => f.PickRandom(rooms).RoomId)
                .RuleFor(r => r.UserId, f => f.PickRandom(users).UserId)
                .RuleFor(r => r.Purpose, f => f.Company.CatchPhrase())
                .RuleFor(r => r.Status, f => f.PickRandom(new[] { "Pending", "Approved", "Rejected", "Cancelled" }))
                .RuleFor(r => r.CreatedAt, f => DateTime.UtcNow.AddDays(-f.Random.Number(1, 60)));

            var newReservations = new List<Reservation>();
            var dateRandom = new Random(123456);

            for (int i = 0; i < 100; i++)
            {
                var res = reservationFaker.Generate();

                var start = DateTime.UtcNow.AddDays(dateRandom.Next(-10, 30)).Date.AddHours(dateRandom.Next(8, 16));
                res.StartTimeUtc = start;
                res.EndTimeUtc = start.AddHours(1);

                if (res.Status == "Approved" || res.Status == "Rejected")
                {
                    res.ApprovedBy = 1;
                }

                if (i % 3 == 0 && res.RoomId.HasValue)
                {
                    var roomEq = allDbEquipment.Where(e => e.RoomId == res.RoomId).ToList();
                    if (roomEq.Any())
                    {
                        res.Equipments.Add(roomEq[dateRandom.Next(roomEq.Count)]);
                    }
                }

                newReservations.Add(res);
            }

            await _db.Reservations.AddRangeAsync(newReservations);
            await _db.SaveChangesAsync();

            if (!await _db.Reviews.AnyAsync())
            {
                var reviewFaker = new Faker<Review>()
                    .RuleFor(r => r.UserId, f => f.PickRandom(users).UserId)
                    .RuleFor(r => r.RoomId, f => f.PickRandom(rooms).RoomId)
                    .RuleFor(r => r.Rating, f => f.Random.Number(1, 5))
                    .RuleFor(r => r.Comment, f => f.Rant.Review())
                    .RuleFor(r => r.CreatedAt, f => DateTime.UtcNow.AddDays(-f.Random.Number(1, 30)));

                var reviews = reviewFaker.Generate(40);
                await _db.Reviews.AddRangeAsync(reviews);
                await _db.SaveChangesAsync();
            }
        }
    }
}