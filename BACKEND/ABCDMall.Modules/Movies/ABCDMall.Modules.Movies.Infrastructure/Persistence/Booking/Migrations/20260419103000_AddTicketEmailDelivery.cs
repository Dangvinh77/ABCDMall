using System;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Migrations
{
    [DbContext(typeof(MoviesBookingDbContext))]
    [Migration("20260419103000_AddTicketEmailDelivery")]
    public partial class AddTicketEmailDelivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAtUtc",
                schema: "movies",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailSendError",
                schema: "movies",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfFileName",
                schema: "movies",
                table: "Tickets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                schema: "movies",
                table: "Tickets",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailSentAtUtc",
                schema: "movies",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EmailSendError",
                schema: "movies",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PdfFileName",
                schema: "movies",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                schema: "movies",
                table: "Tickets");
        }
    }
}
