using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ReservationService(RoomBookerDbContext db)
        {
            _db = db;
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
            // 1. Walidacja prostych rzeczy
            if (dto.EndTimeUtc <= dto.StartTimeUtc)
                throw new InvalidOperationException("Czas zakończenia musi być po czasie rozpoczęcia.");

            // 2. Sprawdzenie kolizji z innymi rezerwacjami
            bool overlaps = await _db.Reservations.AnyAsync(r =>
                r.RoomId == dto.RoomId &&
                r.Status != "Cancelled" &&
                r.Status != "Rejected" &&
                r.StartTimeUtc < dto.EndTimeUtc &&
                dto.StartTimeUtc < r.EndTimeUtc);

            if (overlaps)
                throw new InvalidOperationException("W tym przedziale czasowym sala jest już zarezerwowana.");

            // 3. Sprawdzenie kolizji z maintenance
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

            // Log
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = dto.UserId,
                EntityType = "Reservation",
                EntityId = reservation.ReservationId, // będzie ustawione po SaveChanges
                Action = "Create",
                Details = $"Nowa rezerwacja pokoju {dto.RoomId} {dto.StartTimeUtc:u}–{dto.EndTimeUtc:u}"
            });

            await _db.SaveChangesAsync();

            dto.ReservationId = reservation.ReservationId;
            dto.Status = reservation.Status;
            return dto;
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

