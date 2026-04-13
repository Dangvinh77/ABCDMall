using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class InitMoviesCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "movies");

            migrationBuilder.CreateTable(
                name: "Cinemas",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinemas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Synopsis = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    PosterUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrailerUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RatingLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DefaultLanguage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Biography = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Halls",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CinemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HallType = table.Column<int>(type: "int", nullable: false),
                    SeatCapacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Halls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Halls_Cinemas_CinemaId",
                        column: x => x.CinemaId,
                        principalSchema: "movies",
                        principalTable: "Cinemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieGenres",
                schema: "movies",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieGenres", x => new { x.MovieId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_MovieGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalSchema: "movies",
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieGenres_Movies_MovieId",
                        column: x => x.MovieId,
                        principalSchema: "movies",
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieCredits",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieCredits_Movies_MovieId",
                        column: x => x.MovieId,
                        principalSchema: "movies",
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieCredits_People_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "movies",
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HallSeats",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowLabel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ColumnNumber = table.Column<int>(type: "int", nullable: false),
                    SeatType = table.Column<int>(type: "int", nullable: false),
                    CoupleGroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallSeats_Halls_HallId",
                        column: x => x.HallId,
                        principalSchema: "movies",
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Showtimes",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CinemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Language = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Showtimes_Cinemas_CinemaId",
                        column: x => x.CinemaId,
                        principalSchema: "movies",
                        principalTable: "Cinemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Showtimes_Halls_HallId",
                        column: x => x.HallId,
                        principalSchema: "movies",
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Showtimes_Movies_MovieId",
                        column: x => x.MovieId,
                        principalSchema: "movies",
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShowtimeSeatInventory",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowtimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallSeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowLabel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ColumnNumber = table.Column<int>(type: "int", nullable: false),
                    SeatType = table.Column<int>(type: "int", nullable: false),
                    CoupleGroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowtimeSeatInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShowtimeSeatInventory_HallSeats_HallSeatId",
                        column: x => x.HallSeatId,
                        principalSchema: "movies",
                        principalTable: "HallSeats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShowtimeSeatInventory_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalSchema: "movies",
                        principalTable: "Showtimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cinemas_City_IsActive",
                schema: "movies",
                table: "Cinemas",
                columns: new[] { "City", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Cinemas_Code",
                schema: "movies",
                table: "Cinemas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name",
                schema: "movies",
                table: "Genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Halls_CinemaId_HallCode",
                schema: "movies",
                table: "Halls",
                columns: new[] { "CinemaId", "HallCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HallSeats_HallId_RowLabel_ColumnNumber",
                schema: "movies",
                table: "HallSeats",
                columns: new[] { "HallId", "RowLabel", "ColumnNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HallSeats_HallId_SeatCode",
                schema: "movies",
                table: "HallSeats",
                columns: new[] { "HallId", "SeatCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieCredits_MovieId_PersonId_CreditType",
                schema: "movies",
                table: "MovieCredits",
                columns: new[] { "MovieId", "PersonId", "CreditType" });

            migrationBuilder.CreateIndex(
                name: "IX_MovieCredits_PersonId",
                schema: "movies",
                table: "MovieCredits",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieGenres_GenreId",
                schema: "movies",
                table: "MovieGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Slug",
                schema: "movies",
                table: "Movies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Status_ReleaseDate",
                schema: "movies",
                table: "Movies",
                columns: new[] { "Status", "ReleaseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_CinemaId_BusinessDate_Status",
                schema: "movies",
                table: "Showtimes",
                columns: new[] { "CinemaId", "BusinessDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_HallId_StartAtUtc",
                schema: "movies",
                table: "Showtimes",
                columns: new[] { "HallId", "StartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_MovieId_BusinessDate_Status",
                schema: "movies",
                table: "Showtimes",
                columns: new[] { "MovieId", "BusinessDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeatInventory_HallSeatId",
                schema: "movies",
                table: "ShowtimeSeatInventory",
                column: "HallSeatId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeatInventory_ShowtimeId_SeatCode",
                schema: "movies",
                table: "ShowtimeSeatInventory",
                columns: new[] { "ShowtimeId", "SeatCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeatInventory_ShowtimeId_Status",
                schema: "movies",
                table: "ShowtimeSeatInventory",
                columns: new[] { "ShowtimeId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieCredits",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "MovieGenres",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "ShowtimeSeatInventory",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "People",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Genres",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "HallSeats",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Showtimes",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Halls",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Movies",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Cinemas",
                schema: "movies");
        }
    }
}
