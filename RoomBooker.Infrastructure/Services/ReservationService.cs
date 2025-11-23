using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Dtos;
using RoomBooker.Core.Entities;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Data;

namespace RoomBooker.Infrastructure.Services
{
    public class ReservationService : IReservationService
    {
        private readonly RoomBookerDbContext _db;
        private readonly GoogleAuthService _googleService; 
        public ReservationService(RoomBookerDbContext db, GoogleAuthService googleService)
        {
            _db = db;
            _googleService = googleService;
        }

        public async Task<IEnumerable<ReservationDto>> GetForRoomAsync(int roomId)
        {
            return await _db.Reservations
                .Where(r => r.RoomId == roomId)
                .OrderByDescending(r => r.StartTimeUtc)
                .Select(r => new ReservationDto
                {
                    ReservationId = r.ReservationId,
                    RoomId = r.RoomId,
                    UserId = r.UserId,
                    ApprovedBy = r.ApprovedBy,
                    StartTimeUtc = r.StartTimeUtc,
                    EndTimeUtc = r.EndTimeUtc,
                    Purpose = r.Purpose,
                    Status = r.Status,
                    RejectionReason = r.RejectionReason
                })
                .ToListAsync();
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return null;

            return new ReservationDto
            {
                ReservationId = r.ReservationId,
                RoomId = r.RoomId,
                UserId = r.UserId,
                ApprovedBy = r.ApprovedBy,
                StartTimeUtc = r.StartTimeUtc,
                EndTimeUtc = r.EndTimeUtc,
                Purpose = r.Purpose,
                Status = r.Status,
                RejectionReason = r.RejectionReason
            };
        }

        public async Task<ReservationDto> CreateAsync(ReservationDto dto)
        {
            if (dto.EndTimeUtc <= dto.StartTimeUtc)
                throw new InvalidOperationException("Czas zakończenia musi być po czasie rozpoczęcia.");

            using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                bool overlaps = await _db.Reservations.AnyAsync(r =>
                    r.RoomId == dto.RoomId &&
                    r.Status != "Cancelled" &&
                    r.Status != "Rejected" &&
                    r.StartTimeUtc < dto.EndTimeUtc &&
                    dto.StartTimeUtc < r.EndTimeUtc);

                if (overlaps)
                    throw new InvalidOperationException("W tym przedziale czasowym sala jest już zarezerwowana.");

                bool inMaintenance = await _db.MaintenanceWindows.AnyAsync(m =>
                    m.RoomId == dto.RoomId &&
                    m.IsActive &&
                    m.StartTimeUtc < dto.EndTimeUtc &&
                    dto.StartTimeUtc < m.EndTimeUtc);

                if (inMaintenance)
                    throw new InvalidOperationException("W tym czasie jest zaplanowana przerwa techniczna.");

                var reservation = new Reservation
                {
                    RoomId = dto.RoomId,
                    UserId = dto.UserId,
                    StartTimeUtc = dto.StartTimeUtc,
                    EndTimeUtc = dto.EndTimeUtc,
                    Purpose = dto.Purpose,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Reservations.Add(reservation);
                await _db.SaveChangesAsync();

                _db.AuditLogs.Add(new AuditLog
                {
                    UserId = dto.UserId,
                    EntityType = "Reservation",
                    EntityId = reservation.ReservationId,
                    Action = "Create",
                    Details = $"Nowa rezerwacja pokoju {dto.RoomId}"
                });

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                try
                {
                    var user = await _db.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                    var room = await _db.Rooms.FindAsync(dto.RoomId);
                    if (room != null) reservation.Room = room;

                    if (user != null && !string.IsNullOrEmpty(user.GoogleAccessToken))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _googleService.AddReservationToCalendarAsync(user, reservation);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Google Error - Token wygasł lub inny błąd]: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logic Error]: {ex.Message}");
                }

                dto.ReservationId = reservation.ReservationId;
                dto.Status = reservation.Status;
                return dto;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ApproveAsync(int id, int adminUserId)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;

            if (r.Status != "Pending")
                throw new InvalidOperationException("Tę rezerwację już rozpatrzono.");

            r.Status = "Approved";
            r.ApprovedBy = adminUserId;

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = adminUserId,
                EntityType = "Reservation",
                EntityId = r.ReservationId,
                Action = "Approve",
                Details = "Rezerwacja zatwierdzona."
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id, int adminUserId, string? reason)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;

            if (r.Status != "Pending")
                throw new InvalidOperationException("Tę rezerwację już rozpatrzono.");

            r.Status = "Rejected";
            r.ApprovedBy = adminUserId;
            r.RejectionReason = reason;

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = adminUserId,
                EntityType = "Reservation",
                EntityId = r.ReservationId,
                Action = "Reject",
                Details = $"Rezerwacja odrzucona. Powód: {reason}"
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id, int requestingUserId)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;

            if (r.Status == "Cancelled")
                return true;

            r.Status = "Cancelled";

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = requestingUserId,
                EntityType = "Reservation",
                EntityId = r.ReservationId,
                Action = "Cancel",
                Details = "Rezerwacja anulowana przez użytkownika."
            });

            await _db.SaveChangesAsync();
            return true;
        }
    }
}