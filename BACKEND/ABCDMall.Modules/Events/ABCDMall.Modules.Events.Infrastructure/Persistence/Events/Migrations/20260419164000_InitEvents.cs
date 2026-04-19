using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Events.Infrastructure.Persistence.Events.Migrations
{
    /// <inheritdoc />
    public partial class InitEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "events");

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    ShopId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ShopName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsHot = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventType",
                schema: "events",
                table: "Events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsHot",
                schema: "events",
                table: "Events",
                column: "IsHot");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartDate_EndDate",
                schema: "events",
                table: "Events",
                columns: new[] { "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events",
                schema: "events");
        }
    }
}
