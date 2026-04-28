using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitFoodCourt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "foodcourt");

            migrationBuilder.CreateTable(
                name: "FoodItems",
                schema: "foodcourt",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MallSlug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategorySlug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_Slug",
                schema: "foodcourt",
                table: "FoodItems",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodItems",
                schema: "foodcourt");
        }
    }
}
