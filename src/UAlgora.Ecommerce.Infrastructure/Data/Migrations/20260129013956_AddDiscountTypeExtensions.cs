using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountTypeExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BundleDiscountValue",
                table: "Discounts",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BundleProductIds",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EarlyPaymentDays",
                table: "Discounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCartAbandonmentRecovery",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOverstockClearance",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPointsThreshold",
                table: "Discounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoyaltyTierRequired",
                table: "Discounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferralNewCustomerValue",
                table: "Discounts",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReferralTwoWay",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEmailSubscription",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SeasonLabel",
                table: "Discounts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StandardPaymentDays",
                table: "Discounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TradeInCreditPerItem",
                table: "Discounts",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeInProductIds",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TradeInTargetProductIds",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VolumeTiers",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BundleDiscountValue",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "BundleProductIds",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "EarlyPaymentDays",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "IsCartAbandonmentRecovery",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "IsOverstockClearance",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "LoyaltyPointsThreshold",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "LoyaltyTierRequired",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "ReferralNewCustomerValue",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "ReferralTwoWay",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "RequiresEmailSubscription",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "SeasonLabel",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "StandardPaymentDays",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "TradeInCreditPerItem",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "TradeInProductIds",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "TradeInTargetProductIds",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "VolumeTiers",
                table: "Discounts");
        }
    }
}
