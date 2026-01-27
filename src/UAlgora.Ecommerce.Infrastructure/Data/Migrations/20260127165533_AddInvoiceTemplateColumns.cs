using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTemplateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicensePayments_LicenseSubscriptions_SubscriptionId",
                table: "LicensePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_LicensePayments_Licenses_LicenseId",
                table: "LicensePayments");

            migrationBuilder.AddColumn<string>(
                name: "CompanyGstin",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField1Label",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField1Value",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField2Label",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField2Value",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField3Label",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField3Value",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultPlaceOfSupply",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentTypeCode",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GenerateIrn",
                table: "InvoiceTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HeaderColor",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeaderTextColor",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowAmountInWords",
                table: "InvoiceTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowHsnSacCodes",
                table: "InvoiceTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SignatureImageUrl",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureLabel",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplyTypeCode",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThankYouMessage",
                table: "InvoiceTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgementDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcknowledgementNumber",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmountInWords",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyVatNumber",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField1Label",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField1Value",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField2Label",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField2Value",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField3Label",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomField3Value",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerVatNumber",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentTypeCode",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Irn",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReverseCharge",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QrCodeData",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureImageUrl",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplyTypeCode",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxLabel",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TaxSystem",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_LicensePayments_LicenseSubscriptions_SubscriptionId",
                table: "LicensePayments",
                column: "SubscriptionId",
                principalTable: "LicenseSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LicensePayments_Licenses_LicenseId",
                table: "LicensePayments",
                column: "LicenseId",
                principalTable: "Licenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicensePayments_LicenseSubscriptions_SubscriptionId",
                table: "LicensePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_LicensePayments_Licenses_LicenseId",
                table: "LicensePayments");

            migrationBuilder.DropColumn(
                name: "CompanyGstin",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "CustomField1Label",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "CustomField1Value",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "CustomField2Label",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "CustomField2Value",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "CustomField3Label",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "CustomField3Value",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "DefaultPlaceOfSupply",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "DocumentTypeCode",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "GenerateIrn",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "HeaderColor",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "HeaderTextColor",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "ShowAmountInWords",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "ShowHsnSacCodes",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "SignatureImageUrl",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "SignatureLabel",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "SupplyTypeCode",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "ThankYouMessage",
                table: "InvoiceTemplates");

            migrationBuilder.DropColumn(
                name: "AcknowledgementDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AcknowledgementNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "AmountInWords",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CompanyVatNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomField1Label",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomField1Value",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomField2Label",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomField2Value",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomField3Label",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomField3Value",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerVatNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DocumentTypeCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Irn",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsReverseCharge",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "QrCodeData",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SignatureImageUrl",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SupplyTypeCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TaxLabel",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TaxSystem",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "Invoices");

            migrationBuilder.AddForeignKey(
                name: "FK_LicensePayments_LicenseSubscriptions_SubscriptionId",
                table: "LicensePayments",
                column: "SubscriptionId",
                principalTable: "LicenseSubscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LicensePayments_Licenses_LicenseId",
                table: "LicensePayments",
                column: "LicenseId",
                principalTable: "Licenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
