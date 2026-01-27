using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGstFieldsToTaxCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CgstRate",
                table: "TaxCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GstType",
                table: "TaxCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HsnCode",
                table: "TaxCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IgstRate",
                table: "TaxCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsGst",
                table: "TaxCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SacCode",
                table: "TaxCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SgstRate",
                table: "TaxCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CgstRate",
                table: "TaxCategories");

            migrationBuilder.DropColumn(
                name: "GstType",
                table: "TaxCategories");

            migrationBuilder.DropColumn(
                name: "HsnCode",
                table: "TaxCategories");

            migrationBuilder.DropColumn(
                name: "IgstRate",
                table: "TaxCategories");

            migrationBuilder.DropColumn(
                name: "IsGst",
                table: "TaxCategories");

            migrationBuilder.DropColumn(
                name: "SacCode",
                table: "TaxCategories");

            migrationBuilder.DropColumn(
                name: "SgstRate",
                table: "TaxCategories");
        }
    }
}
