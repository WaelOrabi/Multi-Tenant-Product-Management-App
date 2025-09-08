using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantProductManagementApp.Migrations
{
    /// <inheritdoc />
    public partial class MigrateVariantAttributesToColorSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Migrate legacy JSON stored in Size to proper columns
            //    Only rows where Size contains JSON are updated. Use LEFT to enforce max lengths.
            migrationBuilder.Sql(@"
                UPDATE v
                SET 
                    v.Color = LEFT(JSON_VALUE(v.Size, '$.color'), 50),
                    v.Size  = LEFT(JSON_VALUE(v.Size, '$.size'), 20)
                FROM AppProductVariants v
                WHERE ISJSON(v.Size) = 1
            ");

            // 2) Alter columns to match domain constraints
            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "AppProductVariants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "AppProductVariants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Size",
                table: "AppProductVariants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "AppProductVariants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
