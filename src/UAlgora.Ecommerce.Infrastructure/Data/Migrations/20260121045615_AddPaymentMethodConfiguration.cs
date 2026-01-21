using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentGateways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSandbox = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MerchantId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SandboxApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SandboxSecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SandboxMerchantId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    WebhookUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WebhookSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SandboxWebhookSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebhooksEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SupportedCurrencies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportedCountries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportedPaymentMethods = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatementDescriptor = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: true),
                    StatementDescriptorSuffix = table.Column<string>(type: "nvarchar(22)", maxLength: 22, nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(127)", maxLength: 127, nullable: true),
                    LandingPage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGateways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CheckoutInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GatewayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    FeeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FlatFee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    PercentageFee = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MaxFee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ShowFeeAtCheckout = table.Column<bool>(type: "bit", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxOrderAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AllowedCountries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExcludedCountries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedCurrencies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedCustomerGroups = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShowCardLogos = table.Column<bool>(type: "bit", nullable: false),
                    CssClass = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CaptureMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AutoCaptureDays = table.Column<int>(type: "int", nullable: true),
                    Require3DSecure = table.Column<bool>(type: "bit", nullable: false),
                    RequireCvv = table.Column<bool>(type: "bit", nullable: false),
                    RequireBillingAddress = table.Column<bool>(type: "bit", nullable: false),
                    AllowSavePaymentMethod = table.Column<bool>(type: "bit", nullable: false),
                    AllowRefunds = table.Column<bool>(type: "bit", nullable: false),
                    AllowPartialRefunds = table.Column<bool>(type: "bit", nullable: false),
                    RefundTimeLimitDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_PaymentGateways_GatewayId",
                        column: x => x.GatewayId,
                        principalTable: "PaymentGateways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_Code",
                table: "PaymentGateways",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_IsActive",
                table: "PaymentGateways",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_IsSandbox",
                table: "PaymentGateways",
                column: "IsSandbox");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_Name",
                table: "PaymentGateways",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_ProviderType",
                table: "PaymentGateways",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_SortOrder",
                table: "PaymentGateways",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Code",
                table: "PaymentMethods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_GatewayId",
                table: "PaymentMethods",
                column: "GatewayId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_IsActive",
                table: "PaymentMethods",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_IsDefault",
                table: "PaymentMethods",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_SortOrder",
                table: "PaymentMethods",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Type",
                table: "PaymentMethods",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "PaymentGateways");
        }
    }
}
