using System;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MoviesBookingDbContext))]
    [Migration("20260419090000_AddMovieFeedbackUserFlow")]
    public partial class AddMovieFeedbackUserFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovieFeedbackRequests",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowtimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaserEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AvailableAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvalidatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailRetryCount = table.Column<int>(type: "int", nullable: false),
                    LastEmailError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieFeedbackRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieFeedbackRequests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "movies",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieFeedbacks",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedbackRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowtimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TagsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CreatedByEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ModerationStatus = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModeratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModeratedBy = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ModerationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieFeedbacks_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "movies",
                        principalTable: "Bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovieFeedbacks_MovieFeedbackRequests_FeedbackRequestId",
                        column: x => x.FeedbackRequestId,
                        principalSchema: "movies",
                        principalTable: "MovieFeedbackRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_BookingId_ShowtimeId",
                schema: "movies",
                table: "MovieFeedbackRequests",
                columns: new[] { "BookingId", "ShowtimeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_ExpiresAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_MovieId",
                schema: "movies",
                table: "MovieFeedbackRequests",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_Status_AvailableAtUtc",
                schema: "movies",
                table: "MovieFeedbackRequests",
                columns: new[] { "Status", "AvailableAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbackRequests_TokenHash",
                schema: "movies",
                table: "MovieFeedbackRequests",
                column: "TokenHash",
                unique: true,
                filter: "[TokenHash] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_BookingId",
                schema: "movies",
                table: "MovieFeedbacks",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_FeedbackRequestId",
                schema: "movies",
                table: "MovieFeedbacks",
                column: "FeedbackRequestId",
                unique: true,
                filter: "[FeedbackRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_MovieId_IsVisible_ModerationStatus_CreatedAtUtc",
                schema: "movies",
                table: "MovieFeedbacks",
                columns: new[] { "MovieId", "IsVisible", "ModerationStatus", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_MovieId_Rating_CreatedAtUtc",
                schema: "movies",
                table: "MovieFeedbacks",
                columns: new[] { "MovieId", "Rating", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_ShowtimeId",
                schema: "movies",
                table: "MovieFeedbacks",
                column: "ShowtimeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieFeedbacks",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "MovieFeedbackRequests",
                schema: "movies");
        }
    }
}
