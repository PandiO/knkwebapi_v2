using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FormSubmissionProgresses",
                type: "varchar(255)",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "DisplayConditionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetStepId = table.Column<int>(type: "int", nullable: true),
                    TargetFieldId = table.Column<int>(type: "int", nullable: true),
                    InnerLogic = table.Column<int>(type: "int", nullable: false),
                    CombineWithPreviousLogic = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParentGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayConditionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayConditionGroups_DisplayConditionGroups_ParentGroupId",
                        column: x => x.ParentGroupId,
                        principalTable: "DisplayConditionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisplayConditionGroups_FormFields_TargetFieldId",
                        column: x => x.TargetFieldId,
                        principalTable: "FormFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisplayConditionGroups_FormSteps_TargetStepId",
                        column: x => x.TargetStepId,
                        principalTable: "FormSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "DisplayConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisplayConditionGroupId = table.Column<int>(type: "int", nullable: false),
                    SourceFormFieldId = table.Column<int>(type: "int", nullable: false),
                    SourceFormFieldGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Operator = table.Column<int>(type: "int", nullable: false),
                    ValueJson = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisplayConditions_DisplayConditionGroups_DisplayConditionGro~",
                        column: x => x.DisplayConditionGroupId,
                        principalTable: "DisplayConditionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisplayConditions_FormFields_SourceFormFieldId",
                        column: x => x.SourceFormFieldId,
                        principalTable: "FormFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissionProgresses_Status_CompletedAt",
                table: "FormSubmissionProgresses",
                columns: new[] { "Status", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayConditionGroups_ParentGroupId",
                table: "DisplayConditionGroups",
                column: "ParentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayConditionGroups_TargetFieldId",
                table: "DisplayConditionGroups",
                column: "TargetFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayConditionGroups_TargetStepId",
                table: "DisplayConditionGroups",
                column: "TargetStepId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayConditions_DisplayConditionGroupId",
                table: "DisplayConditions",
                column: "DisplayConditionGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayConditions_SourceFormFieldId",
                table: "DisplayConditions",
                column: "SourceFormFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplayConditions");

            migrationBuilder.DropTable(
                name: "DisplayConditionGroups");

            migrationBuilder.DropIndex(
                name: "IX_FormSubmissionProgresses_Status_CompletedAt",
                table: "FormSubmissionProgresses");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FormSubmissionProgresses",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");
        }
    }
}
