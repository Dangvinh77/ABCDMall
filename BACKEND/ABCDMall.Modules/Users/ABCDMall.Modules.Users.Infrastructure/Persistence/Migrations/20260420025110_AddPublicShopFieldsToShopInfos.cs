using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicShopFieldsToShopInfos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Badge",
                table: "ShopInfos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ShopInfos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "ShopInfos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ShopInfos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Floor",
                table: "ShopInfos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublicVisible",
                table: "ShopInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocationSlot",
                table: "ShopInfos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "ShopInfos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Offer",
                table: "ShopInfos",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenHours",
                table: "ShopInfos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "ShopInfos",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "ShopInfos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "ShopInfos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ShopInfos_Slug",
                table: "ShopInfos",
                column: "Slug",
                unique: true,
                filter: "[Slug] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopInfos_Slug",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Badge",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Floor",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "IsPublicVisible",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "LocationSlot",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Offer",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "OpenHours",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "ShopInfos");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ShopInfos");
        }
    }
}
