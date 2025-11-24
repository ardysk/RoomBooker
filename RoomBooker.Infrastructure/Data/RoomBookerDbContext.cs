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

        // Tables
        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Reservation> Reservations { get; set; } = default!;
        public DbSet<MaintenanceWindow> MaintenanceWindows { get; set; } = default!;
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<Equipment> Equipments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Primary Keys
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<Room>().HasKey(r => r.RoomId);
            modelBuilder.Entity<Reservation>().HasKey(r => r.ReservationId);
            modelBuilder.Entity<MaintenanceWindow>().HasKey(m => m.BlockId);
            modelBuilder.Entity<AuditLog>().HasKey(a => a.LogId);
            modelBuilder.Entity<Equipment>().HasKey(e => e.EquipmentId);

            // Unique Constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //User → Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //User → ApprovedByUser
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.ApprovedByUser)
                .WithMany()
                .HasForeignKey(r => r.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            //Room → Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Room)
                .WithMany(room => room.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            //Room → MaintenanceWindows
            modelBuilder.Entity<MaintenanceWindow>()
                .HasOne(m => m.Room)
                .WithMany(room => room.MaintenanceWindows)
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            //User → AuditLog
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Room)
                .WithMany()
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Room -> Equipment (1:N)
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Room)
                .WithMany(r => r.Equipments)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            //Reservation <-> Equipment (N:M)
            modelBuilder.Entity<Reservation>()
                .HasMany(r => r.Equipments)
                .WithMany(e => e.Reservations);


            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Reservation>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.ActionTimestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            //seed
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@roombooker.local",
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123"),
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

            modelBuilder.Entity<Equipment>().HasData(
                new Equipment { EquipmentId = 1, RoomId = 1, Name = "Projektor 4K" },
                new Equipment { EquipmentId = 2, RoomId = 1, Name = "Tablica Interaktywna" },
                new Equipment { EquipmentId = 3, RoomId = 1, Name = "Zestaw Video-Call" },

                new Equipment { EquipmentId = 4, RoomId = 2, Name = "Telewizor 55 cali" },
                new Equipment { EquipmentId = 5, RoomId = 2, Name = "Flipchart" }
            );
        }
    }
}