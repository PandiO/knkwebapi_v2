using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddDependencyPathToFieldValidationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DependencyPath",
                table: "fieldvalidationrules",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FieldValidationRules_FormFieldId_DependencyPath",
                table: "fieldvalidationrules",
                columns: new[] { "FormFieldId", "DependencyPath" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FieldValidationRules_FormFieldId_DependencyPath",
                table: "fieldvalidationrules");

            migrationBuilder.DropColumn(
                name: "DependencyPath",
                table: "fieldvalidationrules");
        }
    }
}
