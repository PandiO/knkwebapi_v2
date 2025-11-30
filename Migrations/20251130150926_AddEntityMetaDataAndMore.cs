using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityMetaDataAndMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "FormConfigurations",
                newName: "EntityTypeName");

            migrationBuilder.RenameIndex(
                name: "IX_FormConfigurations_EntityName",
                table: "FormConfigurations",
                newName: "IX_FormConfigurations_EntityTypeName");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FormConfigurations",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "FormConfigurations");

            migrationBuilder.RenameColumn(
                name: "EntityTypeName",
                table: "FormConfigurations",
                newName: "EntityName");

            migrationBuilder.RenameIndex(
                name: "IX_FormConfigurations_EntityTypeName",
                table: "FormConfigurations",
                newName: "IX_FormConfigurations_EntityName");
        }
    }
}
