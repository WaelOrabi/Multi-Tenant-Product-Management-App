using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedStocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStocks_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppStocks_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStocks");
        }
    }
}
