using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class AddColorSizeToProductVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AttributesJson",
                table: "AppProductVariants",
                newName: "Size");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "AppProductVariants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "AppProductVariants");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "AppProductVariants",
                newName: "AttributesJson");
        }
    }
}
