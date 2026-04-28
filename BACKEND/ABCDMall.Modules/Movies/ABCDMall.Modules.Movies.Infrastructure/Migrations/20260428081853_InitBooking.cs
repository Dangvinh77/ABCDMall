using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Tickets",
                schema: "movies",
                newName: "Tickets");

            migrationBuilder.RenameTable(
                name: "SnackCombos",
                schema: "movies",
                newName: "SnackCombos");

            migrationBuilder.RenameTable(
                name: "Promotions",
                schema: "movies",
                newName: "Promotions");

            migrationBuilder.RenameTable(
                name: "PromotionRules",
                schema: "movies",
                newName: "PromotionRules");

            migrationBuilder.RenameTable(
                name: "PromotionRedemptions",
                schema: "movies",
                newName: "PromotionRedemptions");

            migrationBuilder.RenameTable(
                name: "Payments",
                schema: "movies",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "OutboxEvents",
                schema: "movies",
                newName: "OutboxEvents");

            migrationBuilder.RenameTable(
                name: "MovieFeedbacks",
                schema: "movies",
                newName: "MovieFeedbacks");

            migrationBuilder.RenameTable(
                name: "MovieFeedbackRequests",
                schema: "movies",
                newName: "MovieFeedbackRequests");

            migrationBuilder.RenameTable(
                name: "GuestCustomers",
                schema: "movies",
                newName: "GuestCustomers");

            migrationBuilder.RenameTable(
                name: "Bookings",
                schema: "movies",
                newName: "Bookings");

            migrationBuilder.RenameTable(
                name: "BookingItems",
                schema: "movies",
                newName: "BookingItems");

            migrationBuilder.RenameTable(
                name: "BookingHoldSeats",
                schema: "movies",
                newName: "BookingHoldSeats");

            migrationBuilder.RenameTable(
                name: "BookingHolds",
                schema: "movies",
                newName: "BookingHolds");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "movies",
                newName: "AuditLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "movies");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "Tickets",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "SnackCombos",
                newName: "SnackCombos",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "Promotions",
                newName: "Promotions",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "PromotionRules",
                newName: "PromotionRules",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "PromotionRedemptions",
                newName: "PromotionRedemptions",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payments",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "OutboxEvents",
                newName: "OutboxEvents",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "MovieFeedbacks",
                newName: "MovieFeedbacks",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "MovieFeedbackRequests",
                newName: "MovieFeedbackRequests",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "GuestCustomers",
                newName: "GuestCustomers",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Bookings",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "BookingItems",
                newName: "BookingItems",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "BookingHoldSeats",
                newName: "BookingHoldSeats",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "BookingHolds",
                newName: "BookingHolds",
                newSchema: "movies");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "AuditLogs",
                newSchema: "movies");
        }
    }
}
