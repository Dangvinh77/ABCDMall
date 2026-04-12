using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Migrations
{
    /// <inheritdoc />
    public partial class InitMoviesBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "movies");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingHolds",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoldCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ShowtimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SeatSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComboSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingHolds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuestCustomers",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValidToUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PercentageValue = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FlatDiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinimumSpendAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxRedemptions = table.Column<int>(type: "int", nullable: true),
                    MaxRedemptionsPerCustomer = table.Column<int>(type: "int", nullable: true),
                    IsAutoApplied = table.Column<bool>(type: "bit", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SnackCombos",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnackCombos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingHoldSeats",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingHoldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatInventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SeatType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoupleGroupCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingHoldSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingHoldSeats_BookingHolds_BookingHoldId",
                        column: x => x.BookingHoldId,
                        principalSchema: "movies",
                        principalTable: "BookingHolds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ShowtimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuestCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BookingHoldId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SeatSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComboSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PromotionSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_GuestCustomers_GuestCustomerId",
                        column: x => x.GuestCustomerId,
                        principalSchema: "movies",
                        principalTable: "GuestCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PromotionRedemptions",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GuestCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RedeemedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionRedemptions_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "movies",
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionRules",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    RuleValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ThresholdValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionRules_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "movies",
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingItems",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SeatInventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItems_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "movies",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CallbackPayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "movies",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeatInventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TicketCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SeatCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QrCodeContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "movies",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId_CreatedAtUtc",
                schema: "movies",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingHolds_HoldCode",
                schema: "movies",
                table: "BookingHolds",
                column: "HoldCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingHolds_ShowtimeId_Status_ExpiresAtUtc",
                schema: "movies",
                table: "BookingHolds",
                columns: new[] { "ShowtimeId", "Status", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingHoldSeats_BookingHoldId_SeatInventoryId",
                schema: "movies",
                table: "BookingHoldSeats",
                columns: new[] { "BookingHoldId", "SeatInventoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingHoldSeats_SeatInventoryId",
                schema: "movies",
                table: "BookingHoldSeats",
                column: "SeatInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_BookingId",
                schema: "movies",
                table: "BookingItems",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItems_SeatInventoryId",
                schema: "movies",
                table: "BookingItems",
                column: "SeatInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingCode",
                schema: "movies",
                table: "Bookings",
                column: "BookingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingHoldId",
                schema: "movies",
                table: "Bookings",
                column: "BookingHoldId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GuestCustomerId",
                schema: "movies",
                table: "Bookings",
                column: "GuestCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ShowtimeId",
                schema: "movies",
                table: "Bookings",
                column: "ShowtimeId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestCustomers_Email",
                schema: "movies",
                table: "GuestCustomers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_GuestCustomers_PhoneNumber",
                schema: "movies",
                table: "GuestCustomers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvents_Status_OccurredAtUtc",
                schema: "movies",
                table: "OutboxEvents",
                columns: new[] { "Status", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                schema: "movies",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderTransactionId",
                schema: "movies",
                table: "Payments",
                column: "ProviderTransactionId",
                unique: true,
                filter: "[ProviderTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                schema: "movies",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRedemptions_BookingId",
                schema: "movies",
                table: "PromotionRedemptions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRedemptions_CouponCode",
                schema: "movies",
                table: "PromotionRedemptions",
                column: "CouponCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRedemptions_GuestCustomerId",
                schema: "movies",
                table: "PromotionRedemptions",
                column: "GuestCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRedemptions_PromotionId",
                schema: "movies",
                table: "PromotionRedemptions",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_PromotionId",
                schema: "movies",
                table: "PromotionRules",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRules_PromotionId_SortOrder",
                schema: "movies",
                table: "PromotionRules",
                columns: new[] { "PromotionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Code",
                schema: "movies",
                table: "Promotions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Status_ValidFromUtc_ValidToUtc",
                schema: "movies",
                table: "Promotions",
                columns: new[] { "Status", "ValidFromUtc", "ValidToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SnackCombos_Code",
                schema: "movies",
                table: "SnackCombos",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnackCombos_IsActive",
                schema: "movies",
                table: "SnackCombos",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingId",
                schema: "movies",
                table: "Tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SeatInventoryId",
                schema: "movies",
                table: "Tickets",
                column: "SeatInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketCode",
                schema: "movies",
                table: "Tickets",
                column: "TicketCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "BookingHoldSeats",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "BookingItems",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "OutboxEvents",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "PromotionRedemptions",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "PromotionRules",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "SnackCombos",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "BookingHolds",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Promotions",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "movies");

            migrationBuilder.DropTable(
                name: "GuestCustomers",
                schema: "movies");
        }
    }
}
