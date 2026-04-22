using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap.Migrations
{
    /// <inheritdoc />
    public partial class AddMapSlotReservationFields : Migration
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
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Available");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
