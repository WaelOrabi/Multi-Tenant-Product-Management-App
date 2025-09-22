using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class Unique_Product_Name_Per_Tenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppProducts_TenantId_Name",
                table: "AppProducts");

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_Name",
                table: "AppProducts",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppProducts_TenantId_Name",
                table: "AppProducts");

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_Name",
                table: "AppProducts",
                columns: new[] { "TenantId", "Name" });
        }
    }
}
