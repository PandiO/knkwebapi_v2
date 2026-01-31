using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddGateAnimationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === Add animation fields to GateStructures table ===
            
            // Gate Type & Motion Configuration
            migrationBuilder.AddColumn<string>(
                name: "GateType",
                table: "gate_structures",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "SLIDING",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AddColumn<string>(
                name: "GeometryDefinitionMode",
                table: "gate_structures",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "PLANE_GRID",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AddColumn<string>(
                name: "MotionType",
                table: "gate_structures",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "VERTICAL",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            // Animation Timing
            migrationBuilder.AddColumn<int>(
                name: "AnimationDurationTicks",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 60);
            
            migrationBuilder.AddColumn<int>(
                name: "AnimationTickRate",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 1);
            
            // PLANE_GRID Geometry
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
            
            migrationBuilder.AddColumn<int>(
                name: "GeometryWidth",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.AddColumn<int>(
                name: "GeometryHeight",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.AddColumn<int>(
                name: "GeometryDepth",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            // FLOOD_FILL Geometry
            migrationBuilder.AddColumn<string>(
                name: "SeedBlocks",
                table: "gate_structures",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AddColumn<int>(
                name: "ScanMaxBlocks",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 500);
            
            migrationBuilder.AddColumn<int>(
                name: "ScanMaxRadius",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 20);
            
            migrationBuilder.AddColumn<string>(
                name: "ScanMaterialWhitelist",
                table: "gate_structures",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AddColumn<string>(
                name: "ScanMaterialBlacklist",
                table: "gate_structures",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AddColumn<bool>(
                name: "ScanPlaneConstraint",
                table: "gate_structures",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
            
            // Block Management
            migrationBuilder.AddColumn<int>(
                name: "FallbackMaterialRefId",
                table: "gate_structures",
                type: "int",
                nullable: true);
            
            migrationBuilder.AddColumn<string>(
                name: "TileEntityPolicy",
                table: "gate_structures",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DECORATIVE_ONLY",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            // Rotation (Drawbridge, Double Doors)
            migrationBuilder.AddColumn<int>(
                name: "RotationMaxAngleDegrees",
                table: "gate_structures",
                type: "int",
                nullable: false,
                defaultValue: 90);
            
            migrationBuilder.AddColumn<string>(
                name: "HingeAxis",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            // Double Doors
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
                name: "RightDoorSeedBlock",
                table: "gate_structures",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "utf8mb4_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AddColumn<bool>(
                name: "MirrorRotation",
                table: "gate_structures",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
            
            // === Create GateBlockSnapshots table ===
            migrationBuilder.CreateTable(
                name: "gate_block_snapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GateStructureId = table.Column<int>(type: "int", nullable: false),
                    RelativeX = table.Column<int>(type: "int", nullable: false),
                    RelativeY = table.Column<int>(type: "int", nullable: false),
                    RelativeZ = table.Column<int>(type: "int", nullable: false),
                    WorldX = table.Column<int>(type: "int", nullable: false),
                    WorldY = table.Column<int>(type: "int", nullable: false),
                    WorldZ = table.Column<int>(type: "int", nullable: false),
                    MaterialName = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlockDataJson = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, defaultValue: "{}", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TileEntityJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, defaultValue: "{}", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gate_block_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gate_block_snapshots_gate_structures_GateStructureId",
                        column: x => x.GateStructureId,
                        principalTable: "gate_structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");
            
            // === Add indexes ===
            
            // GateStructure indexes
            migrationBuilder.CreateIndex(
                name: "IX_GateStructure_IsActive",
                table: "gate_structures",
                column: "IsActive");
            
            migrationBuilder.CreateIndex(
                name: "IX_GateStructure_GateType",
                table: "gate_structures",
                column: "GateType");
            
            migrationBuilder.CreateIndex(
                name: "IX_GateStructure_IsOpened",
                table: "gate_structures",
                column: "IsOpened");
            
            // GateBlockSnapshot indexes
            migrationBuilder.CreateIndex(
                name: "IX_GateBlockSnapshot_GateStructureId",
                table: "gate_block_snapshots",
                column: "GateStructureId");
            
            migrationBuilder.CreateIndex(
                name: "IX_GateBlockSnapshot_GateId_SortOrder",
                table: "gate_block_snapshots",
                columns: new[] { "GateStructureId", "SortOrder" });
            
            migrationBuilder.CreateIndex(
                name: "IX_GateBlockSnapshot_WorldCoordinates",
                table: "gate_block_snapshots",
                columns: new[] { "WorldX", "WorldY", "WorldZ" });
            
            // Foreign key for FallbackMaterialRefId
            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_FallbackMaterialRefId",
                table: "gate_structures",
                column: "FallbackMaterialRefId");

            migrationBuilder.AddForeignKey(
                name: "FK_gate_structures_minecraft_material_refs_FallbackMaterialRefId",
                table: "gate_structures",
                column: "FallbackMaterialRefId",
                principalTable: "minecraft_material_refs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_gate_structures_minecraft_material_refs_FallbackMaterialRefId",
                table: "gate_structures");

            // Drop GateBlockSnapshots table (cascade will handle FK constraint)
            migrationBuilder.DropTable(
                name: "gate_block_snapshots");
            
            // Drop indexes from GateStructures
            migrationBuilder.DropIndex(
                name: "IX_gate_structures_FallbackMaterialRefId",
                table: "gate_structures");
            
            migrationBuilder.DropIndex(
                name: "IX_GateStructure_IsActive",
                table: "gate_structures");
            
            migrationBuilder.DropIndex(
                name: "IX_GateStructure_GateType",
                table: "gate_structures");
            
            migrationBuilder.DropIndex(
                name: "IX_GateStructure_IsOpened",
                table: "gate_structures");
            
            // Drop all animation columns from GateStructures
            migrationBuilder.DropColumn(name: "GateType", table: "gate_structures");
            migrationBuilder.DropColumn(name: "GeometryDefinitionMode", table: "gate_structures");
            migrationBuilder.DropColumn(name: "MotionType", table: "gate_structures");
            migrationBuilder.DropColumn(name: "AnimationDurationTicks", table: "gate_structures");
            migrationBuilder.DropColumn(name: "AnimationTickRate", table: "gate_structures");
            migrationBuilder.DropColumn(name: "AnchorPoint", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ReferencePoint1", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ReferencePoint2", table: "gate_structures");
            migrationBuilder.DropColumn(name: "GeometryWidth", table: "gate_structures");
            migrationBuilder.DropColumn(name: "GeometryHeight", table: "gate_structures");
            migrationBuilder.DropColumn(name: "GeometryDepth", table: "gate_structures");
            migrationBuilder.DropColumn(name: "SeedBlocks", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ScanMaxBlocks", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ScanMaxRadius", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ScanMaterialWhitelist", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ScanMaterialBlacklist", table: "gate_structures");
            migrationBuilder.DropColumn(name: "ScanPlaneConstraint", table: "gate_structures");
            migrationBuilder.DropColumn(name: "FallbackMaterialRefId", table: "gate_structures");
            migrationBuilder.DropColumn(name: "TileEntityPolicy", table: "gate_structures");
            migrationBuilder.DropColumn(name: "RotationMaxAngleDegrees", table: "gate_structures");
            migrationBuilder.DropColumn(name: "HingeAxis", table: "gate_structures");
            migrationBuilder.DropColumn(name: "LeftDoorSeedBlock", table: "gate_structures");
            migrationBuilder.DropColumn(name: "RightDoorSeedBlock", table: "gate_structures");
            migrationBuilder.DropColumn(name: "MirrorRotation", table: "gate_structures");
        }
    }
}
