using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism_project.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomServicesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "RoomServices",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomServices", x => new { x.RoomId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_RoomServices_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

          
            migrationBuilder.CreateIndex(
                name: "IX_RoomServices_ServiceId",
                table: "RoomServices",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomServices");

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

             }
    }
}
