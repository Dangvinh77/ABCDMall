using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieFeedbackLinkLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpiredReason",
                schema: "movies",
                table: "MovieFeedbackRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_LastOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests",
                column: "LastOpenedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_Status_FirstOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests",
                columns: new[] { "Status", "FirstOpenedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovieFeedbackRequests_LastOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests");

            migrationBuilder.DropIndex(
                name: "IX_MovieFeedbackRequests_Status_FirstOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests");

            migrationBuilder.DropColumn(
                name: "ExpiredReason",
                schema: "movies",
                table: "MovieFeedbackRequests");

            migrationBuilder.DropColumn(
                name: "FirstOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests");

            migrationBuilder.DropColumn(
                name: "LastOpenedAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests");
        }
    }
}
