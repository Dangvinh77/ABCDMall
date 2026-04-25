using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Events.Infrastructure.Persistence.Events.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventApprovalStatusDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_EventType",
                schema: "events",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_IsHot",
                schema: "events",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_StartDate_EndDate",
                schema: "events",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "events",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ShopName",
                schema: "events",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "events",
                table: "Events",
                newName: "StartDateTime");

            migrationBuilder.RenameColumn(
                name: "IsHot",
                schema: "events",
                table: "Events",
                newName: "HasGiftRegistration");

            migrationBuilder.RenameColumn(
                name: "EventType",
                schema: "events",
                table: "Events",
                newName: "LocationType");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "events",
                table: "Events",
                newName: "EndDateTime");

            migrationBuilder.RenameColumn(
                name: "CoverImageUrl",
                schema: "events",
                table: "Events",
                newName: "ImageUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "events",
                table: "Events",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                schema: "events",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "GiftDescription",
                schema: "events",
                table: "Events",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

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
                name: "IX_Events_ApprovalStatus_StartDateTime_EndDateTime",
                schema: "events",
                table: "Events",
                columns: new[] { "ApprovalStatus", "StartDateTime", "EndDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_LocationType_StartDateTime_EndDateTime",
                schema: "events",
                table: "Events",
                columns: new[] { "LocationType", "StartDateTime", "EndDateTime" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRegistrations",
                schema: "events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ApprovalStatus_StartDateTime_EndDateTime",
                schema: "events",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_LocationType_StartDateTime_EndDateTime",
                schema: "events",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                schema: "events",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "GiftDescription",
                schema: "events",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                schema: "events",
                table: "Events",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "LocationType",
                schema: "events",
                table: "Events",
                newName: "EventType");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                schema: "events",
                table: "Events",
                newName: "CoverImageUrl");

            migrationBuilder.RenameColumn(
                name: "HasGiftRegistration",
                schema: "events",
                table: "Events",
                newName: "IsHot");

            migrationBuilder.RenameColumn(
                name: "EndDateTime",
                schema: "events",
                table: "Events",
                newName: "EndDate");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "events",
                table: "Events",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "events",
                table: "Events",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                schema: "events",
                table: "Events",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

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
    }
}
