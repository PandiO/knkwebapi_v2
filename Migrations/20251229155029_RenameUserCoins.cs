using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace knkwebapi_v2.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserCoins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cash",
                table: "users",
                newName: "Gems");

            migrationBuilder.AddColumn<int>(
                name: "Coins",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coins",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Gems",
                table: "users",
                newName: "cash");
        }
    }
}
