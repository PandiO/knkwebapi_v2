using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentDomainId",
                table: "domains",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_domains_ParentDomainId",
                table: "domains",
                column: "ParentDomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_domains_domains_ParentDomainId",
                table: "domains",
                column: "ParentDomainId",
                principalTable: "domains",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_domains_domains_ParentDomainId",
                table: "domains");

            migrationBuilder.DropIndex(
                name: "IX_domains_ParentDomainId",
                table: "domains");

            migrationBuilder.DropColumn(
                name: "ParentDomainId",
                table: "domains");
        }
    }
}
