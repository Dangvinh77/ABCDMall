using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops.Migrations
{
    /// <inheritdoc />
    public partial class AddShopStatusAndOpeningDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OpeningDate",
                schema: "shops",
                table: "Shops",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopStatus",
                schema: "shops",
                table: "Shops",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningDate",
                schema: "shops",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ShopStatus",
                schema: "shops",
                table: "Shops");
        }
    }
}
