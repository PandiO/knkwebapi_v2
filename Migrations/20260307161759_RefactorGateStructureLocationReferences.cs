using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class RefactorGateStructureLocationReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnchorPoint",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "GuardSpawnLocationsJson",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "HingeAxis",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "LeftDoorSeedBlock",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "ReferencePoint1",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "ReferencePoint2",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "RightDoorSeedBlock",
                table: "gate_structures");

            migrationBuilder.AddColumn<int>(
                name: "AnchorPointId",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HingeAxisId",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeftDoorSeedBlockId",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferencePoint1Id",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferencePoint2Id",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RightDoorSeedBlockId",
                table: "gate_structures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gate_structure_guard_spawn_locations",
                columns: table => new
                {
                    GateStructureId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gate_structure_guard_spawn_locations", x => new { x.GateStructureId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_gate_structure_guard_spawn_locations_gate_structures_GateStr~",
                        column: x => x.GateStructureId,
                        principalTable: "gate_structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gate_structure_guard_spawn_locations_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_AnchorPointId",
                table: "gate_structures",
                column: "AnchorPointId");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_HingeAxisId",
                table: "gate_structures",
                column: "HingeAxisId");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_LeftDoorSeedBlockId",
                table: "gate_structures",
                column: "LeftDoorSeedBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_ReferencePoint1Id",
                table: "gate_structures",
                column: "ReferencePoint1Id");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_ReferencePoint2Id",
                table: "gate_structures",
                column: "ReferencePoint2Id");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_RightDoorSeedBlockId",
                table: "gate_structures",
                column: "RightDoorSeedBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structure_guard_spawn_locations_LocationId",
                table: "gate_structure_guard_spawn_locations",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_AnchorPointId",
                table: "gate_structures",
                column: "AnchorPointId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_HingeAxisId",
                table: "gate_structures",
                column: "HingeAxisId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_LeftDoorSeedBlockId",
                table: "gate_structures",
                column: "LeftDoorSeedBlockId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_ReferencePoint1Id",
                table: "gate_structures",
                column: "ReferencePoint1Id",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_ReferencePoint2Id",
                table: "gate_structures",
                column: "ReferencePoint2Id",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_locations_RightDoorSeedBlockId",
                table: "gate_structures",
                column: "RightDoorSeedBlockId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_AnchorPointId",
                table: "gate_structures");

            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_HingeAxisId",
                table: "gate_structures");

            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_LeftDoorSeedBlockId",
                table: "gate_structures");

            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_ReferencePoint1Id",
                table: "gate_structures");

            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_ReferencePoint2Id",
                table: "gate_structures");

            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_locations_RightDoorSeedBlockId",
                table: "gate_structures");

            migrationBuilder.DropTable(
                name: "gate_structure_guard_spawn_locations");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_AnchorPointId",
                table: "gate_structures");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_HingeAxisId",
                table: "gate_structures");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_LeftDoorSeedBlockId",
                table: "gate_structures");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_ReferencePoint1Id",
                table: "gate_structures");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_ReferencePoint2Id",
                table: "gate_structures");

            migrationBuilder.DropIndex(
                name: "IX_gate_structures_RightDoorSeedBlockId",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "AnchorPointId",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "HingeAxisId",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "LeftDoorSeedBlockId",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "ReferencePoint1Id",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "ReferencePoint2Id",
                table: "gate_structures");

            migrationBuilder.DropColumn(
                name: "RightDoorSeedBlockId",
                table: "gate_structures");

            migrationBuilder.AddColumn<string>(
                name: "AnchorPoint",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuardSpawnLocationsJson",
                table: "gate_structures",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HingeAxis",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LeftDoorSeedBlock",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReferencePoint1",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReferencePoint2",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RightDoorSeedBlock",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
