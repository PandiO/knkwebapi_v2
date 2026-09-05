using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddGateInfoDisplayLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InfoDisplayLocationId",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_InfoDisplayLocationId",
                table: "gate_structures",
                column: "InfoDisplayLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_InfoDisplayLocationId",
                table: "gate_structures",
                column: "InfoDisplayLocationId",
                principalTable: "locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_InfoDisplayLocationId",
                table: "gate_structures");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_InfoDisplayLocationId",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "InfoDisplayLocationId",
                table: "gate_structures");
        }
    }
}
