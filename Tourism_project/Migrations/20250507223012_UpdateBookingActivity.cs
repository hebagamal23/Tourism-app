using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism_project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookingActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "78a5959c-b6c4-40aa-af73-70a9a40618b5", "4e5ef6fa-bc1e-4ca6-8434-33e9d3e458ec" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "78a5959c-b6c4-40aa-af73-70a9a40618b5");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4e5ef6fa-bc1e-4ca6-8434-33e9d3e458ec");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivityDate",
                table: "BookingActivities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "NumberOfGuests",
                table: "BookingActivities",
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
                keyValues: new object[] { "485f1973-5273-4e9a-bd4c-61fefa8a5003", "0f48ae6e-1fc7-4c99-baa4-b2f312b8243c" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "485f1973-5273-4e9a-bd4c-61fefa8a5003");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0f48ae6e-1fc7-4c99-baa4-b2f312b8243c");

            migrationBuilder.DropColumn(
                name: "ActivityDate",
                table: "BookingActivities");

            migrationBuilder.DropColumn(
                name: "NumberOfGuests",
                table: "BookingActivities");

            }
    }
}
