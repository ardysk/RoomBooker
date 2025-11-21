using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllAsync(bool includeInactive = false);
        Task<RoomDto?> GetByIdAsync(int id);
        Task<RoomDto> CreateAsync(RoomDto dto);
        Task<RoomDto?> UpdateAsync(int id, RoomDto dto);
        Task<bool> DeactivateAsync(int id);
    }
}
