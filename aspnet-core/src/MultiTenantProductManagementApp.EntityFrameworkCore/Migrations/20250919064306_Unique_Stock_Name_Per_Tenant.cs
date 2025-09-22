using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class Unique_Stock_Name_Per_Tenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppStocks_TenantId_Name",
                table: "AppStocks",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppStocks_TenantId_Name",
                table: "AppStocks");
        }
    }
}
