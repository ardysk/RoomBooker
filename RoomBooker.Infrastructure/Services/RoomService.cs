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
            var query = _db.Rooms.AsQueryable();

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            return await query
                .Select(r => new RoomDto
                {
                    RoomId = r.RoomId,
                    Name = r.Name,
                    Capacity = r.Capacity,
                    EquipmentDescription = r.EquipmentDescription,
                    IsActive = r.IsActive
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
                IsActive = dto.IsActive
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            dto.RoomId = room.RoomId;
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
    }
}
