using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MoviesBookingDbContext))]
    [Migration("20260423120000_AllowThreeMovieFeedbackSubmissions")]
    public partial class AllowThreeMovieFeedbackSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovieFeedbacks_FeedbackRequestId",
                schema: "movies",
                table: "MovieFeedbacks");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_FeedbackRequestId",
                schema: "movies",
                table: "MovieFeedbacks",
                column: "FeedbackRequestId",
                filter: "[FeedbackRequestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovieFeedbacks_FeedbackRequestId",
                schema: "movies",
                table: "MovieFeedbacks");

            migrationBuilder.CreateIndex(
                name: "IX_MovieFeedbacks_FeedbackRequestId",
                schema: "movies",
                table: "MovieFeedbacks",
                column: "FeedbackRequestId",
                unique: true,
                filter: "[FeedbackRequestId] IS NOT NULL");
        }
    }
}
