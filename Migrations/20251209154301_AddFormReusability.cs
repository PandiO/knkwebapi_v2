using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddFormReusability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemtypeId",
                table: "categories");

            migrationBuilder.AddColumn<bool>(
                name: "IsLinkedToSource",
                table: "FormSteps",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLinkedToSource",
                table: "FormFields",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLinkedToSource",
                table: "FormSteps");

            migrationBuilder.DropColumn(
                name: "IsLinkedToSource",
                table: "FormFields");

            migrationBuilder.AddColumn<int>(
                name: "ItemtypeId",
                table: "categories",
                type: "int",
                nullable: true);
        }
    }
}
