using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tourism_project.Migrations
{
    /// <inheritdoc />
    public partial class AddTableCartTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "e47675f1-6d1a-4416-a65f-77435013cb24", "01f89121-d7b5-487a-b4bc-4b7d0f24c42a" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e47675f1-6d1a-4416-a65f-77435013cb24");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01f89121-d7b5-487a-b4bc-4b7d0f24c42a");

               }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

             }
    }
}
