using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddEnchantmentDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnchantmentDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayName = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCustom = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxLevel = table.Column<int>(type: "int", nullable: false),
                    MinecraftEnchantmentRefId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnchantmentDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnchantmentDefinitions_minecraftenchantmentrefs_MinecraftEnc~",
                        column: x => x.MinecraftEnchantmentRefId,
                        principalTable: "minecraftenchantmentrefs",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "FieldValidationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FormFieldId = table.Column<int>(type: "int", nullable: false),
                    ValidationType = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DependsOnFieldId = table.Column<int>(type: "int", nullable: true),
                    ConfigJson = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuccessMessage = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBlocking = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiresDependencyFilled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldValidationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldValidationRules_FormFields_DependsOnFieldId",
                        column: x => x.DependsOnFieldId,
                        principalTable: "FormFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FieldValidationRules_FormFields_FormFieldId",
                        column: x => x.FormFieldId,
                        principalTable: "FormFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "ItemBlueprints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconMaterialRefId = table.Column<int>(type: "int", nullable: true),
                    IconMaterialId = table.Column<int>(type: "int", nullable: true),
                    DefaultDisplayName = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultDisplayDescription = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultQuantity = table.Column<int>(type: "int", nullable: false),
                    MaxStackSize = table.Column<int>(type: "int", nullable: false),
                    DefaultEnchantmentIds = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBlueprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemBlueprints_minecraftmaterialrefs_IconMaterialId",
                        column: x => x.IconMaterialId,
                        principalTable: "minecraftmaterialrefs",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "ItemBlueprintDefaultEnchantment",
                columns: table => new
                {
                    ItemBlueprintId = table.Column<int>(type: "int", nullable: false),
                    EnchantmentDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBlueprintDefaultEnchantment", x => new { x.ItemBlueprintId, x.EnchantmentDefinitionId });
                    table.ForeignKey(
                        name: "FK_ItemBlueprintDefaultEnchantment_EnchantmentDefinitions_Encha~",
                        column: x => x.EnchantmentDefinitionId,
                        principalTable: "EnchantmentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemBlueprintDefaultEnchantment_ItemBlueprints_ItemBlueprint~",
                        column: x => x.ItemBlueprintId,
                        principalTable: "ItemBlueprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_EnchantmentDefinitions_MinecraftEnchantmentRefId",
                table: "EnchantmentDefinitions",
                column: "MinecraftEnchantmentRefId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldValidationRules_DependsOnFieldId",
                table: "FieldValidationRules",
                column: "DependsOnFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldValidationRules_FormFieldId",
                table: "FieldValidationRules",
                column: "FormFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBlueprintDefaultEnchantment_EnchantmentDefinitionId",
                table: "ItemBlueprintDefaultEnchantment",
                column: "EnchantmentDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBlueprints_IconMaterialId",
                table: "ItemBlueprints",
                column: "IconMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldValidationRules");

            migrationBuilder.DropTable(
                name: "ItemBlueprintDefaultEnchantment");

            migrationBuilder.DropTable(
                name: "EnchantmentDefinitions");

            migrationBuilder.DropTable(
                name: "ItemBlueprints");
        }
    }
}
