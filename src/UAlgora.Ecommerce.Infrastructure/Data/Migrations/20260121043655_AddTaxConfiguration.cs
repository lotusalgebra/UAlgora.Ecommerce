using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UAlgora.Ecommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxExempt = table.Column<bool>(type: "bit", nullable: false),
                    ExternalTaxCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Countries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    States = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCodePatterns = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExcludedCountries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExcludedStates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExcludedPostalCodes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    RateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FlatAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsCompound = table.Column<bool>(type: "bit", nullable: false),
                    TaxShipping = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaximumTax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    JurisdictionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JurisdictionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JurisdictionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRates_TaxCategories_TaxCategoryId",
                        column: x => x.TaxCategoryId,
                        principalTable: "TaxCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxRates_TaxZones_TaxZoneId",
                        column: x => x.TaxZoneId,
                        principalTable: "TaxZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxCategories_Code",
                table: "TaxCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxCategories_IsActive",
                table: "TaxCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCategories_IsActive_SortOrder",
                table: "TaxCategories",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxCategories_IsDefault",
                table: "TaxCategories",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCategories_SortOrder",
                table: "TaxCategories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_EffectiveFrom_EffectiveTo",
                table: "TaxRates",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_IsActive",
                table: "TaxRates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_IsActive_Priority",
                table: "TaxRates",
                columns: new[] { "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Priority",
                table: "TaxRates",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TaxCategoryId",
                table: "TaxRates",
                column: "TaxCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TaxZoneId",
                table: "TaxRates",
                column: "TaxZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TaxZoneId_TaxCategoryId",
                table: "TaxRates",
                columns: new[] { "TaxZoneId", "TaxCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxZones_Code",
                table: "TaxZones",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxZones_IsActive",
                table: "TaxZones",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TaxZones_IsActive_Priority",
                table: "TaxZones",
                columns: new[] { "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxZones_IsDefault",
                table: "TaxZones",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_TaxZones_Priority",
                table: "TaxZones",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_TaxZones_SortOrder",
                table: "TaxZones",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.DropTable(
                name: "TaxCategories");

            migrationBuilder.DropTable(
                name: "TaxZones");
        }
    }
}
