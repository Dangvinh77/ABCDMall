using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Migrations
{
    /// <inheritdoc />
    public partial class CreateBookingFromHold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingHoldId",
                schema: "movies",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "ComboSnapshotJson",
                schema: "movies",
                table: "BookingHolds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromotionId",
                schema: "movies",
                table: "BookingHolds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromotionSnapshotJson",
                schema: "movies",
                table: "BookingHolds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceFee",
                schema: "movies",
                table: "BookingHolds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingHoldId",
                schema: "movies",
                table: "Bookings",
                column: "BookingHoldId",
                unique: true,
                filter: "[BookingHoldId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BookingHolds_PromotionId",
                schema: "movies",
                table: "BookingHolds",
                column: "PromotionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_BookingHoldId",
                schema: "movies",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingHolds_PromotionId",
                schema: "movies",
                table: "BookingHolds");

            migrationBuilder.DropColumn(
                name: "ComboSnapshotJson",
                schema: "movies",
                table: "BookingHolds");

            migrationBuilder.DropColumn(
                name: "PromotionId",
                schema: "movies",
                table: "BookingHolds");

            migrationBuilder.DropColumn(
                name: "PromotionSnapshotJson",
                schema: "movies",
                table: "BookingHolds");

            migrationBuilder.DropColumn(
                name: "ServiceFee",
                schema: "movies",
                table: "BookingHolds");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingHoldId",
                schema: "movies",
                table: "Bookings",
                column: "BookingHoldId");
        }
    }
}
