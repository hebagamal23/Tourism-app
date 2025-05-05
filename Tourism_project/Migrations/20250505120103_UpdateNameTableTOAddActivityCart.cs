using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism_project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameTableTOAddActivityCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "b1c83d9c-f446-45a9-946b-b35190bcbe03", "2af1e004-d441-4b6c-a89c-71ac67094e65" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b1c83d9c-f446-45a9-946b-b35190bcbe03");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2af1e004-d441-4b6c-a89c-71ac67094e65");

            migrationBuilder.CreateTable(
                name: "AddActivityToCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActivityImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddActivityToCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddActivityToCarts_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

          migrationBuilder.CreateIndex(
                name: "IX_AddActivityToCarts_ActivityId",
                table: "AddActivityToCarts",
                column: "ActivityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddActivityToCarts");

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

            migrationBuilder.CreateTable(
                name: "TripCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripCarts_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.Cascade);
                });

           
            migrationBuilder.CreateIndex(
                name: "IX_TripCarts_ActivityId",
                table: "TripCarts",
                column: "ActivityId");
        }
    }
}
