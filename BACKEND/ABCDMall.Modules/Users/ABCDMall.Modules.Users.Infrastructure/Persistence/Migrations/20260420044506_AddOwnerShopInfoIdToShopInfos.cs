using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerShopInfoIdToShopInfos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerShopInfoId",
                table: "ShopInfos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopInfos_OwnerShopInfoId",
                table: "ShopInfos",
                column: "OwnerShopInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopInfos_OwnerShopInfoId",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "OwnerShopInfoId",
                table: "ShopInfos");
        }
    }
}
