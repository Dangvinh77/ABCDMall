using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopInfoIdToRentalAreas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShopInfoId",
                table: "RentalAreas",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalAreas_ShopInfoId",
                table: "RentalAreas",
                column: "ShopInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RentalAreas_ShopInfoId",
                table: "RentalAreas");

            migrationBuilder.DropColumn(
                name: "ShopInfoId",
                table: "RentalAreas");
        }
    }
}
