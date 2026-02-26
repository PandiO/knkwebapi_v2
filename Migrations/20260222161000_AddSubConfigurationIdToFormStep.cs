using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using knkwebapi_v2.Properties;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    [DbContext(typeof(KnKDbContext))]
    [Migration("20260222161000_AddSubConfigurationIdToFormStep")]
    public partial class AddSubConfigurationIdToFormStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubConfigurationId",
                table: "FormSteps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormSteps_SubConfigurationId",
                table: "FormSteps",
                column: "SubConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormSteps_FormConfigurations_SubConfigurationId",
                table: "FormSteps",
                column: "SubConfigurationId",
                principalTable: "FormConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormSteps_FormConfigurations_SubConfigurationId",
                table: "FormSteps");

            migrationBuilder.DropIndex(
                name: "IX_FormSteps_SubConfigurationId",
                table: "FormSteps");

            migrationBuilder.DropColumn(
                name: "SubConfigurationId",
                table: "FormSteps");
        }
    }
}
