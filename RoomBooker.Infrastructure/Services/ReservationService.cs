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
                    RoomId = r.RoomId ?? 0,
                    RoomName = r.Room != null ? r.Room.Name : "Wypożyczenie sprzętu",
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
            var r = await _db.Reservations.Include(x => x.Room).FirstOrDefaultAsync(x => x.ReservationId == id);
            if (r == null) return null;

            return new ReservationDto
            {
                ReservationId = r.ReservationId,
                RoomId = r.RoomId ?? 0,
                RoomName = r.Room?.Name,
                UserId = r.UserId,
                ApprovedBy = r.ApprovedBy,
                StartTimeUtc = r.StartTimeUtc,
                EndTimeUtc = r.EndTimeUtc,
                Purpose = r.Purpose,
                Status = r.Status,
                RejectionReason = r.RejectionReason
            };
        }

        // ZMIANA: Używamy ReservationCreateDto
        public async Task<ReservationDto> CreateAsync(ReservationCreateDto dto)
        {
            // 1. Walidacja dat
            if (dto.EndTimeUtc <= dto.StartTimeUtc)
                throw new InvalidOperationException("Czas zakończenia musi być po czasie rozpoczęcia.");

            // ROZPOCZYNAMY TRANSAKCJĘ
            using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // 2. SPRAWDZANIE SALI (Tylko jeśli wybrano salę - ID > 0)
                // Używamy .Value, bo RoomId jest int?
                if (dto.RoomId.HasValue && dto.RoomId.Value > 0)
                {
                    bool overlaps = await _db.Reservations.AnyAsync(r =>
                        r.RoomId == dto.RoomId.Value &&
                        r.Status != "Cancelled" &&
                        r.Status != "Rejected" &&
                        r.StartTimeUtc < dto.EndTimeUtc &&
                        dto.StartTimeUtc < r.EndTimeUtc);

                    if (overlaps)
                        throw new InvalidOperationException("W tym przedziale czasowym sala jest już zarezerwowana.");

                    bool inMaintenance = await _db.MaintenanceWindows.AnyAsync(m =>
                        m.RoomId == dto.RoomId.Value &&
                        m.IsActive &&
                        m.StartTimeUtc < dto.EndTimeUtc &&
                        dto.StartTimeUtc < m.EndTimeUtc);

                    if (inMaintenance)
                        throw new InvalidOperationException("W tym czasie jest zaplanowana przerwa techniczna sali.");
                }

                // 3. SPRAWDZANIE SPRZĘTU
                if (dto.SelectedEquipmentIds != null && dto.SelectedEquipmentIds.Any())
                {
                    bool equipmentBusy = await _db.Reservations
                        .AnyAsync(r =>
                            r.Status != "Cancelled" &&
                            r.Status != "Rejected" &&
                            r.StartTimeUtc < dto.EndTimeUtc &&
                            dto.StartTimeUtc < r.EndTimeUtc &&
                            r.Equipments.Any(e => dto.SelectedEquipmentIds.Contains(e.EquipmentId))
                        );

                    if (equipmentBusy)
                        throw new InvalidOperationException("Jeden z wybranych sprzętów jest już zajęty w tym terminie!");
                }

                // Walidacja: Musi być albo sala, albo sprzęt
                if ((dto.RoomId == null || dto.RoomId <= 0) && (dto.SelectedEquipmentIds == null || !dto.SelectedEquipmentIds.Any()))
                {
                    throw new InvalidOperationException("Musisz wybrać salę lub co najmniej jeden przedmiot do wypożyczenia.");
                }

                // 4. Tworzenie encji
                var reservation = new Reservation
                {
                    // Jeśli RoomId <= 0, zapisujemy null (wynajem samego sprzętu)
                    RoomId = (dto.RoomId.HasValue && dto.RoomId.Value > 0) ? dto.RoomId.Value : (int?)null,
                    UserId = dto.UserId, // Zakładamy, że UserId jest wstrzyknięte do DTO przez kontroler
                    StartTimeUtc = dto.StartTimeUtc,
                    EndTimeUtc = dto.EndTimeUtc,
                    Purpose = dto.Purpose,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                // Dodawanie sprzętu do rezerwacji
                if (dto.SelectedEquipmentIds != null && dto.SelectedEquipmentIds.Any())
                {
                    var equipmentToAdd = await _db.Equipments
                        .Where(e => dto.SelectedEquipmentIds.Contains(e.EquipmentId))
                        .ToListAsync();

                    foreach (var item in equipmentToAdd)
                    {
                        reservation.Equipments.Add(item);
                    }

                    // Dopisek do celu rezerwacji
                    if (equipmentToAdd.Any())
                    {
                        var names = string.Join(", ", equipmentToAdd.Select(e => e.Name));
                        reservation.Purpose += $" [Sprzęt: {names}]";
                    }
                }

                _db.Reservations.Add(reservation);
                await _db.SaveChangesAsync();

                // 5. Audit Log
                _db.AuditLogs.Add(new AuditLog
                {
                    UserId = dto.UserId,
                    EntityType = "Reservation",
                    EntityId = reservation.ReservationId,
                    Action = "Create",
                    Details = (dto.RoomId.HasValue && dto.RoomId.Value > 0) ? $"Rezerwacja sali {dto.RoomId}" : "Wypożyczenie sprzętu"
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // --------------------------------------------------------
                // 👇 INTEGRACJA GOOGLE (Poprawiona dla braku sali) 👇
                // --------------------------------------------------------
                try
                {
                    var user = await _db.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == dto.UserId);

                    // Jeśli jest sala, pobieramy ją. Jeśli nie - tworzymy atrapę nazwy.
                    if (reservation.RoomId.HasValue)
                    {
                        var room = await _db.Rooms.FindAsync(reservation.RoomId.Value);
                        if (room != null) reservation.Room = room;
                    }
                    else
                    {
                        reservation.Room = new Room { Name = "Wypożyczenie sprzętu (Bez sali)" };
                    }

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
                                Console.WriteLine($"[Google Error]: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logic Error]: {ex.Message}");
                }
                // --------------------------------------------------------

                // Zwracamy ReservationDto
                return new ReservationDto
                {
                    ReservationId = reservation.ReservationId,
                    Status = reservation.Status,
                    RoomId = reservation.RoomId ?? 0,
                    UserId = reservation.UserId,
                    StartTimeUtc = reservation.StartTimeUtc,
                    EndTimeUtc = reservation.EndTimeUtc,
                    Purpose = reservation.Purpose
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // --- Pozostałe metody (Approve, Reject, Cancel, GetForEquipment) bez zmian ---
        public async Task<bool> ApproveAsync(int id, int adminUserId)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;
            if (r.Status != "Pending") throw new InvalidOperationException("Już rozpatrzono.");
            r.Status = "Approved";
            r.ApprovedBy = adminUserId;
            _db.AuditLogs.Add(new AuditLog { UserId = adminUserId, EntityType = "Reservation", EntityId = id, Action = "Approve" });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id, int adminUserId, string? reason)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;
            if (r.Status != "Pending") throw new InvalidOperationException("Już rozpatrzono.");
            r.Status = "Rejected";
            r.ApprovedBy = adminUserId;
            r.RejectionReason = reason;
            _db.AuditLogs.Add(new AuditLog { UserId = adminUserId, EntityType = "Reservation", EntityId = id, Action = "Reject", Details = reason });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id, int requestingUserId)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;
            if (r.Status == "Cancelled") return true;
            r.Status = "Cancelled";
            _db.AuditLogs.Add(new AuditLog { UserId = requestingUserId, EntityType = "Reservation", EntityId = id, Action = "Cancel" });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReservationDto>> GetForEquipmentAsync(int equipmentId)
        {
            return await _db.Reservations
               .Where(r => r.Equipments.Any(e => e.EquipmentId == equipmentId))
               .OrderByDescending(r => r.StartTimeUtc)
               .Select(r => new ReservationDto
               {
                   ReservationId = r.ReservationId,
                   RoomId = r.RoomId ?? 0,
                   RoomName = r.Room != null ? r.Room.Name : "Wypożyczenie",
                   UserId = r.UserId,
                   StartTimeUtc = r.StartTimeUtc,
                   EndTimeUtc = r.EndTimeUtc,
                   Purpose = r.Purpose,
                   Status = r.Status
               })
               .ToListAsync();
        }
    }
}