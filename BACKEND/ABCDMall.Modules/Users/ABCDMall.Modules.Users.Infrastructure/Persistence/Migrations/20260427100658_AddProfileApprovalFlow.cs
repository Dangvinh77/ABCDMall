using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileApprovalFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileUpdateRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CurrentFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrentCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrentCccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrentCccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestedAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestedCccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedCccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    ReviewedByAdminId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileUpdateRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileUpdateRequests_Status",
                table: "ProfileUpdateRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileUpdateRequests_UserId_Status",
                table: "ProfileUpdateRequests",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileUpdateRequests");
        }
    }
}
