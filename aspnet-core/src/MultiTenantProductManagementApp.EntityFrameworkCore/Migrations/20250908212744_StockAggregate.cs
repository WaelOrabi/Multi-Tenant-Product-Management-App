using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class StockAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStocks_AppProductVariants_ProductVariantId",
                table: "AppStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStocks_AppProducts_ProductId",
                table: "AppStocks");

            migrationBuilder.DropIndex(
                name: "IX_AppStocks_ProductId",
                table: "AppStocks");

            migrationBuilder.DropIndex(
                name: "IX_AppStocks_ProductVariantId",
                table: "AppStocks");

            migrationBuilder.DropIndex(
                name: "IX_AppStocks_TenantId_ProductId_ProductVariantId",
                table: "AppStocks");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "AppStocks");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppStocks");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "AppStocks");

            migrationBuilder.CreateTable(
                name: "AppStockProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStockProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStockProducts_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppStockProducts_AppStocks_StockId",
                        column: x => x.StockId,
                        principalTable: "AppStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStockProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StockProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStockProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStockProductVariants_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppStockProductVariants_AppStockProducts_StockProductId",
                        column: x => x.StockProductId,
                        principalTable: "AppStockProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockProducts_ProductId",
                table: "AppStockProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockProducts_StockId",
                table: "AppStockProducts",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockProducts_TenantId_StockId_ProductId",
                table: "AppStockProducts",
                columns: new[] { "TenantId", "StockId", "ProductId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockProductVariants_ProductVariantId",
                table: "AppStockProductVariants",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockProductVariants_StockProductId",
                table: "AppStockProductVariants",
                column: "StockProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockProductVariants_TenantId_StockProductId_ProductVariantId",
                table: "AppStockProductVariants",
                columns: new[] { "TenantId", "StockProductId", "ProductVariantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [ProductVariantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStockProductVariants");

            migrationBuilder.DropTable(
                name: "AppStockProducts");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "AppStocks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppStocks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "AppStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AppStocks_ProductId",
                table: "AppStocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocks_ProductVariantId",
                table: "AppStocks",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStocks_TenantId_ProductId_ProductVariantId",
                table: "AppStocks",
                columns: new[] { "TenantId", "ProductId", "ProductVariantId" },
                unique: true,
                filter: "[TenantId] IS NOT NULL AND [ProductVariantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStocks_AppProductVariants_ProductVariantId",
                table: "AppStocks",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStocks_AppProducts_ProductId",
                table: "AppStocks",
                column: "ProductId",
                principalTable: "AppProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
