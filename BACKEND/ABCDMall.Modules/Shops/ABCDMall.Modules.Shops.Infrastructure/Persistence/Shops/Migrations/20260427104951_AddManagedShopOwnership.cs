using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops.Migrations
{
    /// <inheritdoc />
    public partial class AddManagedShopOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerShopId",
                schema: "shops",
                table: "Shops",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_OwnerShopId",
                schema: "shops",
                table: "Shops",
                column: "OwnerShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_ShopStatus",
                schema: "shops",
                table: "Shops",
                column: "ShopStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_OwnerShopId",
                schema: "shops",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_ShopStatus",
                schema: "shops",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "OwnerShopId",
                schema: "shops",
                table: "Shops");
        }
    }
}
