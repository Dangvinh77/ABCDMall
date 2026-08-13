using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalStripePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAtUtc",
                table: "ShopMonthlyBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ShopMonthlyBills",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "ShopMonthlyBills",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "ShopMonthlyBills",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAtUtc",
                table: "ShopMonthlyBills");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ShopMonthlyBills");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "ShopMonthlyBills");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "ShopMonthlyBills");
        }
    }
}
