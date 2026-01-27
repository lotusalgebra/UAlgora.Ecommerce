using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseSubscriptionAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LicenseSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PaymentProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderSubscriptionId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProviderPriceId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    BillingInterval = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "year"),
                    CurrentPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelAtPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    PaymentCount = table.Column<int>(type: "int", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicensedDomain = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LicenseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    LastFailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseSubscriptions_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicensePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LicenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderPaymentId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProviderInvoiceId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProviderChargeId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    CustomerEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InvoiceUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    RefundReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "subscription"),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CardBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CardCountry = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RawResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicensePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicensePayments_LicenseSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "LicenseSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LicensePayments_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_CreatedAt",
                table: "LicensePayments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_CustomerEmail",
                table: "LicensePayments",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_LicenseId",
                table: "LicensePayments",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_LicenseType",
                table: "LicensePayments",
                column: "LicenseType");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_PaidAt",
                table: "LicensePayments",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_PaymentProvider",
                table: "LicensePayments",
                column: "PaymentProvider");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_PaymentProvider_Status",
                table: "LicensePayments",
                columns: new[] { "PaymentProvider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_ProviderChargeId",
                table: "LicensePayments",
                column: "ProviderChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_ProviderCustomerId",
                table: "LicensePayments",
                column: "ProviderCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_ProviderInvoiceId",
                table: "LicensePayments",
                column: "ProviderInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_ProviderPaymentId",
                table: "LicensePayments",
                column: "ProviderPaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_Status",
                table: "LicensePayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_Status_CreatedAt",
                table: "LicensePayments",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LicensePayments_SubscriptionId",
                table: "LicensePayments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_CreatedAt",
                table: "LicenseSubscriptions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_CurrentPeriodEnd",
                table: "LicenseSubscriptions",
                column: "CurrentPeriodEnd");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_CustomerEmail",
                table: "LicenseSubscriptions",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_LicenseId",
                table: "LicenseSubscriptions",
                column: "LicenseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_LicenseType",
                table: "LicenseSubscriptions",
                column: "LicenseType");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_NextPaymentDate",
                table: "LicenseSubscriptions",
                column: "NextPaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_PaymentProvider",
                table: "LicenseSubscriptions",
                column: "PaymentProvider");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_PaymentProvider_Status",
                table: "LicenseSubscriptions",
                columns: new[] { "PaymentProvider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_ProviderCustomerId",
                table: "LicenseSubscriptions",
                column: "ProviderCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_ProviderSubscriptionId",
                table: "LicenseSubscriptions",
                column: "ProviderSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_Status",
                table: "LicenseSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseSubscriptions_Status_AutoRenew",
                table: "LicenseSubscriptions",
                columns: new[] { "Status", "AutoRenew" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicensePayments");

            migrationBuilder.DropTable(
                name: "LicenseSubscriptions");
        }
    }
}
