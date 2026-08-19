using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemBlueprintDefaultEnchantment_ItemBlueprints_ItemBlueprint~",
                table: "ItemBlueprintDefaultEnchantment");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemBlueprints_minecraftmaterialrefs_IconMaterialId",
                table: "ItemBlueprints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemBlueprints",
                table: "ItemBlueprints");

            migrationBuilder.DropIndex(
                name: "IX_ItemBlueprints_IconMaterialId",
                table: "ItemBlueprints");

            migrationBuilder.DropColumn(
                name: "IconMaterialId",
                table: "ItemBlueprints");

            migrationBuilder.RenameTable(
                name: "ItemBlueprints",
                newName: "item_blueprints");

            migrationBuilder.AddPrimaryKey(
                name: "PRIMARY",
                table: "item_blueprints",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "game_settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SettingsVersion = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JoinAnnouncement = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeaveAnnouncement = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JoinSpawnMode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JoinSpawnReferenceJson = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultRespawnPolicyJson = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorldSettingsJson = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuntimeWorldsJson = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RuntimeWorldsLastUpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_settings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "gate_structures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanRespawn = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDestroyed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsInvincible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsOpened = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HealthCurrent = table.Column<double>(type: "double", nullable: false),
                    HealthMax = table.Column<double>(type: "double", nullable: false),
                    FaceDirection = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RespawnRateSeconds = table.Column<int>(type: "int", nullable: false),
                    IconMaterialRefId = table.Column<int>(type: "int", nullable: true),
                    RegionClosedId = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegionOpenedId = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GateType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeometryDefinitionMode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotionType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnimationDurationTicks = table.Column<int>(type: "int", nullable: false),
                    AnimationTickRate = table.Column<int>(type: "int", nullable: false),
                    AnchorPoint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferencePoint1 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferencePoint2 = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeometryWidth = table.Column<int>(type: "int", nullable: false),
                    GeometryHeight = table.Column<int>(type: "int", nullable: false),
                    GeometryDepth = table.Column<int>(type: "int", nullable: false),
                    SeedBlocks = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScanMaxBlocks = table.Column<int>(type: "int", nullable: false),
                    ScanMaxRadius = table.Column<int>(type: "int", nullable: false),
                    ScanMaterialWhitelist = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScanMaterialBlacklist = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScanPlaneConstraint = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FallbackMaterialRefId = table.Column<int>(type: "int", nullable: true),
                    TileEntityPolicy = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RotationMaxAngleDegrees = table.Column<int>(type: "int", nullable: false),
                    HingeAxis = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LeftDoorSeedBlock = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RightDoorSeedBlock = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MirrorRotation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowPassThrough = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PassThroughDurationSeconds = table.Column<int>(type: "int", nullable: false),
                    PassThroughConditionsJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuardSpawnLocationsJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GuardCount = table.Column<int>(type: "int", nullable: false),
                    GuardNpcTemplateId = table.Column<int>(type: "int", nullable: true),
                    ShowHealthDisplay = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HealthDisplayMode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HealthDisplayYOffset = table.Column<int>(type: "int", nullable: false),
                    IsOverridable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AnimateDuringSiege = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CurrentSiegeId = table.Column<int>(type: "int", nullable: true),
                    IsSiegeObjective = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowContinuousDamage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContinuousDamageMultiplier = table.Column<double>(type: "double", nullable: false),
                    ContinuousDamageDurationSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gate_structures_minecraftmaterialrefs_FallbackMaterialRefId",
                        column: x => x.FallbackMaterialRefId,
                        principalTable: "minecraftmaterialrefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gate_structures_minecraftmaterialrefs_IconMaterialRefId",
                        column: x => x.IconMaterialRefId,
                        principalTable: "minecraftmaterialrefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_gate_structures_structures_Id",
                        column: x => x.Id,
                        principalTable: "structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

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
                    BlockDataJson = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TileEntityJson = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false, collation: "utf8mb4_general_ci")
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

            migrationBuilder.CreateIndex(
                name: "IX_item_blueprints_IconMaterialRefId",
                table: "item_blueprints",
                column: "IconMaterialRefId");

            migrationBuilder.CreateIndex(
                name: "IX_GateBlockSnapshot_GateId_SortOrder",
                table: "gate_block_snapshots",
                columns: new[] { "GateStructureId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_GateBlockSnapshot_GateStructureId",
                table: "gate_block_snapshots",
                column: "GateStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_GateBlockSnapshot_WorldCoordinates",
                table: "gate_block_snapshots",
                columns: new[] { "WorldX", "WorldY", "WorldZ" });

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_FallbackMaterialRefId",
                table: "gate_structures",
                column: "FallbackMaterialRefId");

            migrationBuilder.CreateIndex(
                name: "IX_gate_structures_IconMaterialRefId",
                table: "gate_structures",
                column: "IconMaterialRefId");

            migrationBuilder.CreateIndex(
                name: "IX_GateStructure_GateType",
                table: "gate_structures",
                column: "GateType");

            migrationBuilder.CreateIndex(
                name: "IX_GateStructure_IsActive",
                table: "gate_structures",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GateStructure_IsOpened",
                table: "gate_structures",
                column: "IsOpened");

            migrationBuilder.AddForeignKey(
                name: "FK_item_blueprints_minecraftmaterialrefs_IconMaterialRefId",
                table: "item_blueprints",
                column: "IconMaterialRefId",
                principalTable: "minecraftmaterialrefs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemBlueprintDefaultEnchantment_item_blueprints_ItemBlueprin~",
                table: "ItemBlueprintDefaultEnchantment",
                column: "ItemBlueprintId",
                principalTable: "item_blueprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_blueprints_minecraftmaterialrefs_IconMaterialRefId",
                table: "item_blueprints");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemBlueprintDefaultEnchantment_item_blueprints_ItemBlueprin~",
                table: "ItemBlueprintDefaultEnchantment");

            migrationBuilder.DropTable(
                name: "game_settings");

            migrationBuilder.DropTable(
                name: "gate_block_snapshots");

            migrationBuilder.DropTable(
                name: "gate_structures");

            migrationBuilder.DropPrimaryKey(
                name: "PRIMARY",
                table: "item_blueprints");

            migrationBuilder.DropIndex(
                name: "IX_item_blueprints_IconMaterialRefId",
                table: "item_blueprints");

            migrationBuilder.RenameTable(
                name: "item_blueprints",
                newName: "ItemBlueprints");

            migrationBuilder.AddColumn<int>(
                name: "IconMaterialId",
                table: "ItemBlueprints",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemBlueprints",
                table: "ItemBlueprints",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBlueprints_IconMaterialId",
                table: "ItemBlueprints",
                column: "IconMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemBlueprintDefaultEnchantment_ItemBlueprints_ItemBlueprint~",
                table: "ItemBlueprintDefaultEnchantment",
                column: "ItemBlueprintId",
                principalTable: "ItemBlueprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemBlueprints_minecraftmaterialrefs_IconMaterialId",
                table: "ItemBlueprints",
                column: "IconMaterialId",
                principalTable: "minecraftmaterialrefs",
                principalColumn: "Id");
        }
    }
}
