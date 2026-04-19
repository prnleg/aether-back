using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aether.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Assets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Assets",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "Assets",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Assets");
        }
    }
}
