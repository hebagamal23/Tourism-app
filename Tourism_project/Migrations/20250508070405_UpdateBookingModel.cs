using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism_project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "485f1973-5273-4e9a-bd4c-61fefa8a5003", "0f48ae6e-1fc7-4c99-baa4-b2f312b8243c" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "485f1973-5273-4e9a-bd4c-61fefa8a5003");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0f48ae6e-1fc7-4c99-baa4-b2f312b8243c");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

             }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "a390d93e-f6d3-4e41-8cd2-c7b9b0b827c4", "38e055f0-390a-4de1-a39c-ac1d931e1db0" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a390d93e-f6d3-4e41-8cd2-c7b9b0b827c4");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "38e055f0-390a-4de1-a39c-ac1d931e1db0");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "bookings");
 }
    }
}
