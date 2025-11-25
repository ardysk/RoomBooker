using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Entities;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Data;

namespace RoomBooker.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly RoomBookerDbContext _db;

        public ReviewService(RoomBookerDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ReviewDto>> GetForRoomAsync(int roomId, int currentUserId)
        {
            return await _db.Reviews
                .Where(r => r.RoomId == roomId)
                .Include(r => r.User)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    RoomId = r.RoomId,
                    RoomName = r.Room.Name,
                    UserId = r.UserId,
                    UserDisplayName = r.User.DisplayName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsMyReview = r.UserId == currentUserId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync()
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    RoomId = r.RoomId,
                    RoomName = r.Room.Name,
                    UserId = r.UserId,
                    UserDisplayName = r.User.DisplayName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReviewDto> AddAsync(ReviewCreateDto dto, int userId)
        {

            bool alreadyReviewed = await _db.Reviews.AnyAsync(r => r.RoomId == dto.RoomId && r.UserId == userId);
            if (alreadyReviewed)
                throw new InvalidOperationException("Możesz dodać tylko jedną opinię dla danej sali. Edytuj istniejącą.");

            bool hasReservation = await _db.Reservations.AnyAsync(r =>
                r.RoomId == dto.RoomId &&
                r.UserId == userId &&
                r.EndTimeUtc < DateTime.UtcNow &&
                r.Status != "Cancelled" &&
                r.Status != "Rejected");

            if (!hasReservation)
                throw new InvalidOperationException("Możesz ocenić tylko salę, którą już wcześniej wynajmowałeś/aś.");

            var review = new Review
            {
                RoomId = dto.RoomId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                IsMyReview = true
            };
        }

        public async Task<bool> UpdateAsync(int id, int userId, ReviewCreateDto dto)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return false;

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("Nie możesz edytować cudzej opinii.");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.CreatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int userId, bool isAdmin)
        {
            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return false;

            if (!isAdmin && review.UserId != userId)
                throw new UnauthorizedAccessException("Brak uprawnień do usunięcia.");

            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<ReviewDto>> GetByUserAsync(int userId)
        {
            return await _db.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    RoomId = r.RoomId,
                    RoomName = r.Room.Name,
                    UserId = r.UserId,
                    UserDisplayName = r.User.DisplayName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsMyReview = true
                })
                .ToListAsync();
        }
    }
}