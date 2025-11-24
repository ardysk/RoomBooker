using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Entities;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Data;

namespace RoomBooker.Infrastructure.Services
{
    public class RoomService : IRoomService
    {
        private readonly RoomBookerDbContext _db;

        public RoomService(RoomBookerDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RoomDto>> GetAllAsync(bool includeInactive = false)
        {
            var query = _db.Rooms
                .Include(r => r.Equipments) // <--- WAŻNE: Pobierz sprzęt z bazy!
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            return await query
                .Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    Name = r.Name,
                    Capacity = r.Capacity,
                    EquipmentDescription = r.EquipmentDescription,
                    IsActive = r.IsActive,
                    Equipments = r.Equipments.Select(e => new EquipmentDto
                    {
                        EquipmentId = e.EquipmentId,
                        Name = e.Name,
                        RoomId = e.RoomId
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<RoomDto?> GetByIdAsync(int id)
        {
            var r = await _db.Rooms.FindAsync(id);
            if (r == null) return null;

            return new RoomDto
            {
                RoomId = r.RoomId,
                Name = r.Name,
                Capacity = r.Capacity,
                EquipmentDescription = r.EquipmentDescription,
                IsActive = r.IsActive
            };
        }

        public async Task<RoomDto> CreateAsync(RoomDto dto)
        {
            var room = new Room
            {
                Name = dto.Name,
                Capacity = dto.Capacity,
                EquipmentDescription = dto.EquipmentDescription,
                IsActive = true // Upewnij się, że nowa sala jest aktywna!
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync(); // <--- TO ZAPISUJE DO BAZY

            dto.RoomId = room.RoomId; // Przypisz ID z bazy
            dto.Id = room.RoomId;     // I do aliasu też
            return dto;
        }

        public async Task<RoomDto?> UpdateAsync(int id, RoomDto dto)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return null;

            room.Name = dto.Name;
            room.Capacity = dto.Capacity;
            room.EquipmentDescription = dto.EquipmentDescription;
            room.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();

            dto.RoomId = room.RoomId;
            return dto;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null) return false;

            room.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RoomStatDto>> GetMonthlyStatsAsync(int month, int year)
        {
            var result = await _db.Database
                .SqlQuery<RoomStatDto>($"EXEC dbo.sp_GetMonthlyRoomStats {month}, {year}")
                .ToListAsync();

            return result;
        }
        public async Task<byte[]> GenerateCsvReportAsync(int month, int year)
        {
            var stats = await GetMonthlyStatsAsync(month, year);
            var sb = new System.Text.StringBuilder();

            // Header CSV
            sb.AppendLine("Nazwa Sali,Liczba Rezerwacji,Suma Godzin");

            // Data
            foreach (var s in stats)
            {
                sb.AppendLine($"{s.RoomName},{s.ReservationCount},{s.TotalHours}");
            }

            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
