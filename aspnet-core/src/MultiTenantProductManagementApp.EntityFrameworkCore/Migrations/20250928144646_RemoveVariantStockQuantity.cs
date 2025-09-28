using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVariantStockQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "AppProductVariants");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppStocks",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "AppStocks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "AppStocks");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "AppStocks");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "AppProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
