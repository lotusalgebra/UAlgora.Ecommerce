using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGstFieldsToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CgstAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CgstRate",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CompanyGstin",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerGstin",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GstBreakdownJson",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IgstAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IgstRate",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsGstApplicable",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInterState",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfSupply",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SgstAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SgstRate",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CgstAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CgstRate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CompanyGstin",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerGstin",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "GstBreakdownJson",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IgstAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IgstRate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsGstApplicable",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsInterState",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PlaceOfSupply",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SgstAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SgstRate",
                table: "Invoices");
        }
    }
}
