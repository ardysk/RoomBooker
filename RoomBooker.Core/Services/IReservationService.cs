using System.Collections.Generic;
using System.Threading.Tasks;
using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDto>> GetForRoomAsync(int roomId);
        Task<IEnumerable<ReservationDto>> GetForEquipmentAsync(int equipmentId);

        Task<ReservationDto?> GetByIdAsync(int id);

        Task<ReservationDto> CreateAsync(ReservationCreateDto dto);

        Task<bool> CancelAsync(int id, int requestingUserId);

        Task<bool> ApproveAsync(int id, int adminUserId);
        Task<bool> RejectAsync(int id, int adminUserId, string? reason);
    }
}