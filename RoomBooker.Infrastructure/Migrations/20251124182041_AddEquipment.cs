using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoomBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.EquipmentId);
                    table.ForeignKey(
                        name: "FK_Equipments_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentReservation",
                columns: table => new
                {
                    EquipmentsEquipmentId = table.Column<int>(type: "int", nullable: false),
                    ReservationsReservationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentReservation", x => new { x.EquipmentsEquipmentId, x.ReservationsReservationId });
                    table.ForeignKey(
                        name: "FK_EquipmentReservation_Equipments_EquipmentsEquipmentId",
                        column: x => x.EquipmentsEquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentReservation_Reservations_ReservationsReservationId",
                        column: x => x.ReservationsReservationId,
                        principalTable: "Reservations",
                        principalColumn: "ReservationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Equipments",
                columns: new[] { "EquipmentId", "Name", "RoomId" },
                values: new object[,]
                {
                    { 1, "Projektor 4K", 1 },
                    { 2, "Tablica Interaktywna", 1 },
                    { 3, "Zestaw Video-Call", 1 },
                    { 4, "Telewizor 55 cali", 2 },
                    { 5, "Flipchart", 2 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "HashedPassword",
                value: "$2a$11$wDKm3ufVKHDpczQFKIsJLO49eQ5EBj6amHWvU5PNXpUQjrDKLJQ/6");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentReservation_ReservationsReservationId",
                table: "EquipmentReservation",
                column: "ReservationsReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_RoomId",
                table: "Equipments",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentReservation");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "HashedPassword",
                value: "$2a$11$OSx2yMm8OlJCUWX8qiX6Ce9iJLkf885O3xCOh86bGZOQ1S3Os0eyq");
        }
    }
}
