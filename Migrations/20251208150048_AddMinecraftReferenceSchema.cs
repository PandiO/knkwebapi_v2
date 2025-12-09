using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class AddMinecraftReferenceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IconMaterialRefId",
                table: "categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "minecraftmaterialrefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NamespaceKey = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LegacyName = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "minecraftblockrefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NamespaceKey = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlockStateString = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogicalType = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_minecraftmaterialrefs_NamespaceKey",
                table: "minecraftmaterialrefs",
                column: "NamespaceKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_IconMaterialRefId",
                table: "categories",
                column: "IconMaterialRefId");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_minecraftmaterialrefs_IconMaterialRefId",
                table: "categories",
                column: "IconMaterialRefId",
                principalTable: "minecraftmaterialrefs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_minecraftmaterialrefs_IconMaterialRefId",
                table: "categories");

            migrationBuilder.DropTable(
                name: "minecraftblockrefs");

            migrationBuilder.DropTable(
                name: "minecraftmaterialrefs");

            migrationBuilder.DropIndex(
                name: "IX_categories_IconMaterialRefId",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "IconMaterialRefId",
                table: "categories");
        }
    }
}
