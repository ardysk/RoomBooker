using RoomBooker.Core.Dtos;

namespace RoomBooker.Core.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetForRoomAsync(int roomId, int currentUserId);
        Task<IEnumerable<ReviewDto>> GetAllAsync();

        Task<ReviewDto> AddAsync(ReviewCreateDto dto, int userId);
        Task<bool> UpdateAsync(int id, int userId, ReviewCreateDto dto);
        Task<bool> DeleteAsync(int id, int userId, bool isAdmin);
        Task<IEnumerable<ReviewDto>> GetByUserAsync(int userId);
    }
}