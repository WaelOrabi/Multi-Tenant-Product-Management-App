using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStockName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AppStocks",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AppStocks");
        }
    }
}
