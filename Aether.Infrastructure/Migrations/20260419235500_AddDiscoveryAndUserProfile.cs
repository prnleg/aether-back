using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aether.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscoveryAndUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscoveryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MarketHashName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IconUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    RawMetadata = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MarketPriceAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    MarketPriceCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscoveryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SteamId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscoveryItems_UserId_AppId_ExternalId",
                table: "DiscoveryItems",
                columns: new[] { "UserId", "AppId", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscoveryItems");

            migrationBuilder.DropTable(
                name: "UserProfiles");
        }
    }
}
