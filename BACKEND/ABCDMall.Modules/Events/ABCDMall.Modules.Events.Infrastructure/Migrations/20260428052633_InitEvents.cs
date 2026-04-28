using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Events.Infrastructure.Migrations
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
                    EventType = table.Column<int>(type: "int", nullable: false),
                    ShopId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasGiftRegistration = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    GiftDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                schema: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RedeemCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "events",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId_CustomerEmail",
                schema: "events",
                table: "EventRegistrations",
                columns: new[] { "EventId", "CustomerEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_RedeemCode",
                schema: "events",
                table: "EventRegistrations",
                column: "RedeemCode");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ApprovalStatus_StartDate_EndDate",
                schema: "events",
                table: "Events",
                columns: new[] { "ApprovalStatus", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventType_StartDate_EndDate",
                schema: "events",
                table: "Events",
                columns: new[] { "EventType", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistrations",
                schema: "events");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "events");
        }
    }
}
