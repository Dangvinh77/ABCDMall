using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodManagerAndMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "foodcourt",
                table: "FoodItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "foodcourt",
                table: "FoodItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpenHours",
                schema: "foodcourt",
                table: "FoodItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerShopId",
                schema: "foodcourt",
                table: "FoodItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "foodcourt",
                table: "FoodItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Promo",
                schema: "foodcourt",
                table: "FoodItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FoodMenuItems",
                schema: "foodcourt",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FoodStallId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IngredientsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodMenuItems_FoodItems_FoodStallId",
                        column: x => x.FoodStallId,
                        principalSchema: "foodcourt",
                        principalTable: "FoodItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodMenuItems_FoodStallId_DisplayOrder",
                schema: "foodcourt",
                table: "FoodMenuItems",
                columns: new[] { "FoodStallId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodMenuItems",
                schema: "foodcourt");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "foodcourt",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "foodcourt",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "OpenHours",
                schema: "foodcourt",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "OwnerShopId",
                schema: "foodcourt",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "foodcourt",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "Promo",
                schema: "foodcourt",
                table: "FoodItems");
        }
    }
}
