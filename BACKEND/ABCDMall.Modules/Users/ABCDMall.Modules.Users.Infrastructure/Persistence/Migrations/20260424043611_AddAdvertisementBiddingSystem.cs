using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementBiddingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarouselBids",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    ShopId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TemplateType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TemplateData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TargetMondayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarouselBids", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovieCarouselAds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TargetMondayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCarouselAds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarouselBids_ShopId",
                table: "CarouselBids",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselBids_Status",
                table: "CarouselBids",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CarouselBids_TargetMondayDate",
                table: "CarouselBids",
                column: "TargetMondayDate");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCarouselAds_IsActive",
                table: "MovieCarouselAds",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCarouselAds_TargetMondayDate",
                table: "MovieCarouselAds",
                column: "TargetMondayDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarouselBids");

            migrationBuilder.DropTable(
                name: "MovieCarouselAds");
        }
    }
}
