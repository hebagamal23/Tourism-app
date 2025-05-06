using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism_project.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestsToBookingActivityAndCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "c48ee062-4340-420a-8176-7b27a6d80cbb", "e828e841-2243-4414-9f5a-e63cdc26954b" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c48ee062-4340-420a-8176-7b27a6d80cbb");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "e828e841-2243-4414-9f5a-e63cdc26954b");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfGuests",
                table: "AddActivityToCarts",
                type: "int",
                nullable: false,
                defaultValue: 0);

              }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3978fbe6-e77e-44cc-97ea-865f130365e8", "4a314ddd-ccb5-4fb0-9a85-33206001335f" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3978fbe6-e77e-44cc-97ea-865f130365e8");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4a314ddd-ccb5-4fb0-9a85-33206001335f");

            migrationBuilder.DropColumn(
                name: "NumberOfGuests",
                table: "AddActivityToCarts");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

           }
    }
}
