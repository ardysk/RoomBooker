using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDto>> GetForRoomAsync(int roomId);
        Task<ReservationDto?> GetByIdAsync(int id);

        Task<ReservationDto> CreateAsync(ReservationDto dto);
        Task<bool> ApproveAsync(int id, int adminUserId);
        Task<bool> RejectAsync(int id, int adminUserId, string? reason);
        Task<bool> CancelAsync(int id, int requestingUserId);
    }
}
