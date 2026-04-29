using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForgotPasswordOtps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Otp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NewPasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgotPasswordOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetOtps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Otp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NewPasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetOtps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileUpdateHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PreviousFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreviousAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreviousCccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousCccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedCccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedCccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileUpdateHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileUpdateRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CurrentFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrentCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrentCccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurrentCccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestedAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestedCccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedCccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    ReviewedByAdminId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileUpdateRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RentalAreas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    AreaCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AreaName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShopInfoId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    OwnerShopInfoId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ShopName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LocationSlot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OpenHours = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Badge = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Offer = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPublicVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RentalLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Month = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LeaseStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ElectricityUsage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ElectricityFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WaterUsage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WaterFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeaseTermDays = table.Column<int>(type: "int", nullable: false),
                    TotalDue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContractImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContractImages = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpeningDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopMonthlyBills",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    ShopInfoId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BillKey = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    ShopName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RentalLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Month = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsageMonth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingMonthKey = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsageMonthKey = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LeaseStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ElectricityUsage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ElectricityFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WaterUsage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WaterFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LeaseTermDays = table.Column<int>(type: "int", nullable: false),
                    TotalDue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Unpaid"),
                    StripeSessionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContractImages = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopMonthlyBills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValueSql: "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShopId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CccdFrontImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CccdBackImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LoginOtpCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LoginOtpExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OneTimePasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OneTimePasswordExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OneTimePasswordUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordSetupToken = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PasswordSetupTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordSetupCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileUpdateRequests_Status",
                table: "ProfileUpdateRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileUpdateRequests_UserId_Status",
                table: "ProfileUpdateRequests",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalAreas_AreaCode",
                table: "RentalAreas",
                column: "AreaCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalAreas_ShopInfoId",
                table: "RentalAreas",
                column: "ShopInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopInfos_CCCD",
                table: "ShopInfos",
                column: "CCCD",
                unique: true,
                filter: "[CCCD] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopInfos_OwnerShopInfoId",
                table: "ShopInfos",
                column: "OwnerShopInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopInfos_Slug",
                table: "ShopInfos",
                column: "Slug",
                unique: true,
                filter: "[Slug] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_ShopMonthlyBills_BillKey",
                table: "ShopMonthlyBills",
                column: "BillKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CCCD",
                table: "Users",
                column: "CCCD",
                unique: true,
                filter: "[CCCD] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForgotPasswordOtps");

            migrationBuilder.DropTable(
                name: "PasswordResetOtps");

            migrationBuilder.DropTable(
                name: "ProfileUpdateHistories");

            migrationBuilder.DropTable(
                name: "ProfileUpdateRequests");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RentalAreas");

            migrationBuilder.DropTable(
                name: "ShopInfos");

            migrationBuilder.DropTable(
                name: "ShopMonthlyBills");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
