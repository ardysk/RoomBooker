using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Entities;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Data;

namespace RoomBooker.Infrastructure.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly RoomBookerDbContext _db;

        public EquipmentService(RoomBookerDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<EquipmentDto>> GetAllAsync()
        {
            return await _db.Equipments
                .Include(e => e.Room)
                .Select(e => new EquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    RoomId = e.RoomId,
                    RoomName = e.Room.Name
                })
                .ToListAsync();
        }

        public async Task<EquipmentDto?> GetByIdAsync(int id)
        {
            var e = await _db.Equipments.Include(x => x.Room).FirstOrDefaultAsync(x => x.EquipmentId == id);
            if (e == null) return null;

            return new EquipmentDto
            {
                EquipmentId = e.EquipmentId,
                Name = e.Name,
                RoomId = e.RoomId,
                RoomName = e.Room.Name
            };
        }

        public async Task<EquipmentDto> CreateAsync(EquipmentDto dto)
        {
            var equipment = new Equipment
            {
                Name = dto.Name,
                RoomId = dto.RoomId
            };

            _db.Equipments.Add(equipment);
            await _db.SaveChangesAsync();

            dto.EquipmentId = equipment.EquipmentId;
            var room = await _db.Rooms.FindAsync(dto.RoomId);
            dto.RoomName = room?.Name;

            return dto;
        }

        public async Task<EquipmentDto?> UpdateAsync(int id, EquipmentDto dto)
        {
            var equipment = await _db.Equipments.FindAsync(id);
            if (equipment == null) return null;

            equipment.Name = dto.Name;
            equipment.RoomId = dto.RoomId;

            await _db.SaveChangesAsync();
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var equipment = await _db.Equipments.FindAsync(id);
            if (equipment == null) return false;

            _db.Equipments.Remove(equipment);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}