using System.Collections.Generic;
using System.Threading.Tasks;
using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Services
{
    public interface IEquipmentService
    {
        Task<IEnumerable<EquipmentDto>> GetAllAsync();
        Task<EquipmentDto?> GetByIdAsync(int id);
        Task<EquipmentDto> CreateAsync(EquipmentDto dto);
        Task<EquipmentDto?> UpdateAsync(int id, EquipmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}