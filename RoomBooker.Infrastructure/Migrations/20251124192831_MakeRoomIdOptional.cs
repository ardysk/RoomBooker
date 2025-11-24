using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomBooker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeRoomIdOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Reservations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "HashedPassword",
                value: "$2a$11$cukk4WeGUdkjmyERgRfukeQUpgzah1QpyDez84w1VOqLx5x8HBYay");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "HashedPassword",
                value: "$2a$11$wDKm3ufVKHDpczQFKIsJLO49eQ5EBj6amHWvU5PNXpUQjrDKLJQ/6");
        }
    }
}
