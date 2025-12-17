using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddManyToManyRelationshipSupportToFormStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManyToManyRelationship",
                table: "FormSteps",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JoinEntityType",
                table: "FormSteps",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ParentStepId",
                table: "FormSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityPropertyName",
                table: "FormSteps",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FormSteps_ParentStepId",
                table: "FormSteps",
                column: "ParentStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormSteps_FormSteps_ParentStepId",
                table: "FormSteps",
                column: "ParentStepId",
                principalTable: "FormSteps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormSteps_FormSteps_ParentStepId",
                table: "FormSteps");

            migrationBuilder.DropIndex(
                name: "IX_FormSteps_ParentStepId",
                table: "FormSteps");

            migrationBuilder.DropColumn(
                name: "IsManyToManyRelationship",
                table: "FormSteps");

            migrationBuilder.DropColumn(
                name: "JoinEntityType",
                table: "FormSteps");

            migrationBuilder.DropColumn(
                name: "ParentStepId",
                table: "FormSteps");

            migrationBuilder.DropColumn(
                name: "RelatedEntityPropertyName",
                table: "FormSteps");
        }
    }
}
