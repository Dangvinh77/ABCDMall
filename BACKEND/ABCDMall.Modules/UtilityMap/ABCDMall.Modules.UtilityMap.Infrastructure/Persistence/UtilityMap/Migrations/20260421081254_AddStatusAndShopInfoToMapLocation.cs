using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndShopInfoToMapLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopInfoId",
                schema: "utility_map",
                table: "MapLocations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "utility_map",
                table: "MapLocations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Available");

            migrationBuilder.CreateIndex(
                name: "IX_MapLocations_ShopInfoId",
                schema: "utility_map",
                table: "MapLocations",
                column: "ShopInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_MapLocations_Status",
                schema: "utility_map",
                table: "MapLocations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MapLocations_ShopInfoId",
                schema: "utility_map",
                table: "MapLocations");

            migrationBuilder.DropIndex(
                name: "IX_MapLocations_Status",
                schema: "utility_map",
                table: "MapLocations");

            migrationBuilder.DropColumn(
                name: "ShopInfoId",
                schema: "utility_map",
                table: "MapLocations");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "utility_map",
                table: "MapLocations");
        }
    }
}
