using Microsoft.EntityFrameworkCore;
using RoomBooker.Core.Entities;

namespace RoomBooker.Infrastructure.Data
{
    public class RoomBookerDbContext : DbContext
    {
        public RoomBookerDbContext(DbContextOptions<RoomBookerDbContext> options)
            : base(options)
        {
        }

        // Tabele
        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<MaintenanceWindow> MaintenanceWindows => Set<MaintenanceWindow>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Klucze główne
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Room>().HasKey(r => r.RoomId);
            modelBuilder.Entity<Reservation>().HasKey(r => r.ReservationId);
            modelBuilder.Entity<MaintenanceWindow>().HasKey(m => m.BlockId);
            modelBuilder.Entity<AuditLog>().HasKey(a => a.LogId);

            // Unikalny e-mail
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Relacja User → Reservations (kto złożył rezerwację)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja User → ApprovedByUser (admin zatwierdzający)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.ApprovedByUser)
                .WithMany()
                .HasForeignKey(r => r.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja Room → Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Room)
                .WithMany(room => room.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja Room → MaintenanceWindows
            modelBuilder.Entity<MaintenanceWindow>()
                .HasOne(m => m.Room)
                .WithMany(room => room.MaintenanceWindows)
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacja User → AuditLog
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Domyślne wartości
            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Reservation>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.ActionTimestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            // 👇 Dane startowe (seed) – admin, zwykły user i 2 sale
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@roombooker.local",
                    HashedPassword = "admin-hash-placeholder",
                    DisplayName = "Administrator",
                    Role = "Admin"
                },
                new User
                {
                    UserId = 2,
                    Email = "user@roombooker.local",
                    HashedPassword = "user-hash-placeholder",
                    DisplayName = "Użytkownik",
                    Role = "User"
                }
            );

            modelBuilder.Entity<Room>().HasData(
                new Room
                {
                    RoomId = 1,
                    Name = "Sala Konferencyjna A",
                    Capacity = 20,
                    EquipmentDescription = "Projektor, tablica, nagłośnienie",
                    IsActive = true
                },
                new Room
                {
                    RoomId = 2,
                    Name = "Sala 101",
                    Capacity = 12,
                    EquipmentDescription = "Monitor, tablica suchościeralna",
                    IsActive = true
                }
            );
        }
    }
}
