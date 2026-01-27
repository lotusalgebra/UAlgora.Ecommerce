using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Web.Services;

/// <summary>
/// Service for generating PDF invoices.
/// </summary>
public interface IInvoicePdfService
{
    /// <summary>
    /// Generates a PDF invoice.
    /// </summary>
    byte[] GenerateInvoicePdf(Invoice invoice, InvoiceTemplate? template = null);

    /// <summary>
    /// Generates a PDF packing slip (no prices).
    /// </summary>
    byte[] GeneratePackingSlipPdf(Invoice invoice, InvoiceTemplate? template = null);
}

/// <summary>
/// Implementation of invoice PDF generation using QuestPDF.
/// </summary>
public class InvoicePdfService : IInvoicePdfService
{
    static InvoicePdfService()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    public byte[] GenerateInvoicePdf(Invoice invoice, InvoiceTemplate? template = null)
    {
        template ??= GetDefaultTemplate();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, invoice, template));
                page.Content().Element(c => ComposeContent(c, invoice, template));
                page.Footer().Element(c => ComposeFooter(c, invoice, template));
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GeneratePackingSlipPdf(Invoice invoice, InvoiceTemplate? template = null)
    {
        template ??= GetDefaultTemplate();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposePackingSlipHeader(c, invoice, template));
                page.Content().Element(c => ComposePackingSlipContent(c, invoice, template));
                page.Footer().Element(c => ComposePackingSlipFooter(c, invoice, template));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Invoice invoice, InvoiceTemplate template)
    {
        var headerColor = ParseColor(template.HeaderColor);
        var headerTextColor = ParseColor(template.HeaderTextColor);

        container.Background(headerColor).Padding(20).Row(row =>
        {
            // Left side - Company info
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(invoice.CompanyName)
                    .FontSize(16).Bold().FontColor(headerTextColor);

                if (!string.IsNullOrEmpty(invoice.CompanyAddress1))
                    col.Item().Text(invoice.CompanyAddress1).FontSize(9).FontColor(headerTextColor);
                if (!string.IsNullOrEmpty(invoice.CompanyAddress2))
                    col.Item().Text(invoice.CompanyAddress2).FontSize(9).FontColor(headerTextColor);
                if (!string.IsNullOrEmpty(invoice.CompanyCity) || !string.IsNullOrEmpty(invoice.CompanyState))
                    col.Item().Text($"{invoice.CompanyCity}, {invoice.CompanyState} {invoice.CompanyPostalCode}".Trim(' ', ','))
                        .FontSize(9).FontColor(headerTextColor);
                if (!string.IsNullOrEmpty(invoice.CompanyPhone))
                    col.Item().Text($"Contact: {invoice.CompanyPhone}").FontSize(9).FontColor(headerTextColor);

                // Show tax ID based on tax system
                if (invoice.TaxSystem == TaxSystemType.GST && !string.IsNullOrEmpty(invoice.CompanyGstin))
                    col.Item().Text($"GSTIN: {invoice.CompanyGstin}").FontSize(9).FontColor(headerTextColor);
                else if (invoice.TaxSystem == TaxSystemType.VAT && !string.IsNullOrEmpty(invoice.CompanyVatNumber))
                    col.Item().Text($"VAT No: {invoice.CompanyVatNumber}").FontSize(9).FontColor(headerTextColor);
                else if (!string.IsNullOrEmpty(invoice.TaxId))
                    col.Item().Text($"Tax ID: {invoice.TaxId}").FontSize(9).FontColor(headerTextColor);
            });

            // Right side - Customer info
            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text(invoice.CustomerName)
                    .FontSize(14).Bold().FontColor(headerTextColor).AlignRight();

                var billingAddress = DeserializeAddress(invoice.BillingAddressJson);
                if (billingAddress != null)
                {
                    if (!string.IsNullOrEmpty(billingAddress.Address1))
                        col.Item().Text(billingAddress.Address1).FontSize(9).FontColor(headerTextColor).AlignRight();
                    if (!string.IsNullOrEmpty(billingAddress.City) || !string.IsNullOrEmpty(billingAddress.StateProvince))
                        col.Item().Text($"{billingAddress.City}, {billingAddress.StateProvince}".Trim(' ', ','))
                            .FontSize(9).FontColor(headerTextColor).AlignRight();
                }
                if (!string.IsNullOrEmpty(invoice.CustomerPhone))
                    col.Item().Text($"Contact: {invoice.CustomerPhone}").FontSize(9).FontColor(headerTextColor).AlignRight();

                // Show customer tax ID based on tax system
                if (invoice.TaxSystem == TaxSystemType.GST && !string.IsNullOrEmpty(invoice.CustomerGstin))
                    col.Item().Text($"GSTIN: {invoice.CustomerGstin}").FontSize(9).FontColor(headerTextColor).AlignRight();
                else if (invoice.TaxSystem == TaxSystemType.VAT && !string.IsNullOrEmpty(invoice.CustomerVatNumber))
                    col.Item().Text($"VAT No: {invoice.CustomerVatNumber}").FontSize(9).FontColor(headerTextColor).AlignRight();
            });
        });
    }

    private void ComposeContent(IContainer container, Invoice invoice, InvoiceTemplate template)
    {
        var accentColor = ParseColor(template.AccentColor);

        container.Padding(20).Column(col =>
        {
            // Invoice title and details row
            col.Item().Row(row =>
            {
                // Left side - Title and QR code
                row.RelativeItem().Column(leftCol =>
                {
                    leftCol.Item().Text(template.InvoiceTitle.ToLower() + ".")
                        .FontSize(36).FontColor(Colors.Grey.Darken3);

                    if (template.ShowQrCode && !string.IsNullOrEmpty(invoice.QrCodeData))
                    {
                        leftCol.Item().PaddingTop(10).Width(80).Height(80)
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .AlignCenter().AlignMiddle()
                            .Text("QR").FontSize(8).FontColor(Colors.Grey.Medium);
                    }
                });

                // Right side - Invoice details
                row.RelativeItem().AlignRight().Column(rightCol =>
                {
                    rightCol.Item().Text($"Invoice No.: {invoice.InvoiceNumber}")
                        .FontSize(10).AlignRight();
                    rightCol.Item().Text($"Date: {invoice.IssuedAt.ToString(template.DateFormat)}")
                        .FontSize(10).AlignRight();

                    if (!string.IsNullOrEmpty(invoice.PlaceOfSupply))
                        rightCol.Item().Text($"Place of Supply: {invoice.PlaceOfSupply}")
                            .FontSize(10).AlignRight();

                    if (!string.IsNullOrEmpty(invoice.SupplyTypeCode))
                        rightCol.Item().Text($"Supply Type: {invoice.SupplyTypeCode}")
                            .FontSize(10).AlignRight();

                    if (!string.IsNullOrEmpty(invoice.DocumentTypeCode))
                        rightCol.Item().Text($"Document Type: {invoice.DocumentTypeCode}")
                            .FontSize(10).AlignRight();

                    if (!string.IsNullOrEmpty(invoice.Irn))
                    {
                        rightCol.Item().PaddingTop(10).Text("IRN:").FontSize(9);
                        // Split IRN into multiple lines for readability
                        var irn = invoice.Irn;
                        for (int i = 0; i < irn.Length; i += 32)
                        {
                            var chunk = irn.Substring(i, Math.Min(32, irn.Length - i));
                            rightCol.Item().Text(chunk).FontSize(8).FontColor(Colors.Grey.Darken1).AlignRight();
                        }
                    }
                });
            });

            col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Line items table
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.RelativeColumn(4);   // Item Name
                    columns.RelativeColumn(1);   // Quantity
                    columns.RelativeColumn(1.2f); // Price/Unit
                    columns.RelativeColumn(1.2f); // GST
                    columns.RelativeColumn(1.2f); // Amount
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5)
                        .Text("#").Bold().FontSize(9);
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5)
                        .Text("Item Name").Bold().FontSize(9);
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5)
                        .Text("Quantity").Bold().FontSize(9).AlignRight();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5)
                        .Text("Price/Unit").Bold().FontSize(9).AlignRight();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5)
                        .Text("GST").Bold().FontSize(9).AlignRight();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Padding(5)
                        .Text("Amount").Bold().FontSize(9).AlignRight();
                });

                // Line items
                var lineItems = DeserializeLineItems(invoice.LineItemsJson);
                var index = 1;
                foreach (var item in lineItems)
                {
                    var gstAmount = item.TaxAmount;
                    var gstRate = item.TaxRate;

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(index.ToString()).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Column(c =>
                        {
                            c.Item().Text(item.ProductName).FontSize(9);
                            if (template.ShowHsnSacCodes && !string.IsNullOrEmpty(item.HsnCode))
                                c.Item().Text($"HSN: {item.HsnCode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(item.Quantity.ToString()).FontSize(9).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatCurrency(item.UnitPrice, invoice.CurrencyCode)).FontSize(9).AlignRight();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Column(c =>
                        {
                            c.Item().Text(FormatCurrency(gstAmount, invoice.CurrencyCode)).FontSize(9).AlignRight();
                            if (gstRate > 0)
                                c.Item().Text($"({gstRate}%)").FontSize(8).FontColor(Colors.Grey.Darken1).AlignRight();
                        });
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(FormatCurrency(item.LineTotal, invoice.CurrencyCode)).FontSize(9).AlignRight();

                    index++;
                }
            });

            col.Item().PaddingTop(20);

            // Summary section
            col.Item().Row(row =>
            {
                // Left side - Signature and custom fields
                row.RelativeItem().Column(leftCol =>
                {
                    leftCol.Item().PaddingTop(30).Text("Signature").FontSize(10).FontColor(Colors.Grey.Darken1);

                    if (!string.IsNullOrEmpty(template.SignatureImageUrl))
                    {
                        // Placeholder for signature image
                        leftCol.Item().PaddingTop(5).Width(150).Height(50)
                            .Border(1).BorderColor(Colors.Grey.Lighten2)
                            .AlignCenter().AlignMiddle()
                            .Text("[Signature]").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        leftCol.Item().PaddingTop(30).Width(150).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                    }

                    // Custom fields
                    if (!string.IsNullOrEmpty(invoice.CustomField1Label))
                        leftCol.Item().PaddingTop(15).Text($"{invoice.CustomField1Label}: {invoice.CustomField1Value}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(invoice.CustomField2Label))
                        leftCol.Item().Text($"{invoice.CustomField2Label}: {invoice.CustomField2Value}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(invoice.CustomField3Label))
                        leftCol.Item().Text($"{invoice.CustomField3Label}: {invoice.CustomField3Value}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);

                    // Thank you message
                    leftCol.Item().PaddingTop(20).Text(template.ThankYouMessage)
                        .FontSize(12).Bold().FontColor(accentColor);
                });

                // Right side - Totals
                row.RelativeItem().AlignRight().Column(rightCol =>
                {
                    rightCol.Item().AlignRight().Text("Total Due").FontSize(10).FontColor(Colors.Grey.Darken1);

                    // Grand total in large font
                    rightCol.Item().AlignRight().Text(FormatCurrency(invoice.GrandTotal, invoice.CurrencyCode))
                        .FontSize(32).Bold().FontColor(Colors.Grey.Darken3);

                    // Amount in words
                    if (template.ShowAmountInWords && !string.IsNullOrEmpty(invoice.AmountInWords))
                        rightCol.Item().AlignRight().Text(invoice.AmountInWords)
                            .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);

                    rightCol.Item().PaddingTop(10);

                    // Breakdown
                    rightCol.Item().AlignRight().Row(r =>
                    {
                        r.AutoItem().Text("Sub Total").FontSize(9);
                        r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.Subtotal, invoice.CurrencyCode)).FontSize(9);
                    });

                    // Tax breakdown based on tax system
                    switch (invoice.TaxSystem)
                    {
                        case TaxSystemType.GST:
                            // GST breakdown (India style - CGST + SGST or IGST)
                            if (invoice.IsInterState && invoice.IgstAmount > 0)
                            {
                                rightCol.Item().AlignRight().Row(r =>
                                {
                                    r.AutoItem().Text($"IGST @{invoice.IgstRate}%").FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.IgstAmount, invoice.CurrencyCode)).FontSize(9);
                                });
                            }
                            else
                            {
                                if (invoice.CgstAmount > 0)
                                {
                                    rightCol.Item().AlignRight().Row(r =>
                                    {
                                        r.AutoItem().Text($"CGST @{invoice.CgstRate}%").FontSize(9);
                                        r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.CgstAmount, invoice.CurrencyCode)).FontSize(9);
                                    });
                                }
                                if (invoice.SgstAmount > 0)
                                {
                                    rightCol.Item().AlignRight().Row(r =>
                                    {
                                        r.AutoItem().Text($"SGST @{invoice.SgstRate}%").FontSize(9);
                                        r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.SgstAmount, invoice.CurrencyCode)).FontSize(9);
                                    });
                                }
                            }
                            break;

                        case TaxSystemType.VAT:
                            // VAT breakdown (EU/UK style)
                            if (invoice.IsReverseCharge)
                            {
                                rightCol.Item().AlignRight().Row(r =>
                                {
                                    r.AutoItem().Text("VAT (Reverse Charge)").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    r.ConstantItem(100).AlignRight().Text(FormatCurrency(0, invoice.CurrencyCode)).FontSize(9);
                                });
                            }
                            else if (invoice.VatAmount > 0)
                            {
                                rightCol.Item().AlignRight().Row(r =>
                                {
                                    r.AutoItem().Text($"VAT @{invoice.VatRate}%").FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.VatAmount, invoice.CurrencyCode)).FontSize(9);
                                });
                            }
                            break;

                        case TaxSystemType.SalesTax:
                            // Sales Tax (US style)
                            if (invoice.TaxTotal > 0)
                            {
                                rightCol.Item().AlignRight().Row(r =>
                                {
                                    r.AutoItem().Text($"Sales Tax").FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.TaxTotal, invoice.CurrencyCode)).FontSize(9);
                                });
                            }
                            break;

                        default:
                            // Generic tax or legacy invoices
                            if (invoice.IsGstApplicable)
                            {
                                // Legacy GST handling
                                if (invoice.IsInterState && invoice.IgstAmount > 0)
                                {
                                    rightCol.Item().AlignRight().Row(r =>
                                    {
                                        r.AutoItem().Text($"IGST @{invoice.IgstRate}%").FontSize(9);
                                        r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.IgstAmount, invoice.CurrencyCode)).FontSize(9);
                                    });
                                }
                                else
                                {
                                    if (invoice.CgstAmount > 0)
                                    {
                                        rightCol.Item().AlignRight().Row(r =>
                                        {
                                            r.AutoItem().Text($"CGST @{invoice.CgstRate}%").FontSize(9);
                                            r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.CgstAmount, invoice.CurrencyCode)).FontSize(9);
                                        });
                                    }
                                    if (invoice.SgstAmount > 0)
                                    {
                                        rightCol.Item().AlignRight().Row(r =>
                                        {
                                            r.AutoItem().Text($"SGST @{invoice.SgstRate}%").FontSize(9);
                                            r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.SgstAmount, invoice.CurrencyCode)).FontSize(9);
                                        });
                                    }
                                }
                            }
                            else if (invoice.TaxTotal > 0)
                            {
                                var taxLabel = !string.IsNullOrEmpty(invoice.TaxLabel) ? invoice.TaxLabel : "Tax";
                                rightCol.Item().AlignRight().Row(r =>
                                {
                                    r.AutoItem().Text(taxLabel).FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.TaxTotal, invoice.CurrencyCode)).FontSize(9);
                                });
                            }
                            break;
                    }

                    if (invoice.ShippingTotal > 0)
                    {
                        rightCol.Item().AlignRight().Row(r =>
                        {
                            r.AutoItem().Text("Shipping").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.ShippingTotal, invoice.CurrencyCode)).FontSize(9);
                        });
                    }

                    if (invoice.DiscountTotal > 0)
                    {
                        rightCol.Item().AlignRight().Row(r =>
                        {
                            r.AutoItem().Text("Discount").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text($"-{FormatCurrency(invoice.DiscountTotal, invoice.CurrencyCode)}").FontSize(9).FontColor(Colors.Green.Darken1);
                        });
                    }
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, Invoice invoice, InvoiceTemplate template)
    {
        container.Padding(20).Column(col =>
        {
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                col.Item().Text("Notes:").FontSize(9).Bold();
                col.Item().Text(invoice.Notes).FontSize(9).FontColor(Colors.Grey.Darken1);
            }

            if (!string.IsNullOrEmpty(invoice.Terms))
            {
                col.Item().PaddingTop(10).Text("Terms & Conditions:").FontSize(9).Bold();
                col.Item().Text(invoice.Terms).FontSize(9).FontColor(Colors.Grey.Darken1);
            }

            col.Item().PaddingTop(10).AlignRight().Text("Generated by Algora Commerce")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private void ComposePackingSlipHeader(IContainer container, Invoice invoice, InvoiceTemplate template)
    {
        var headerColor = ParseColor(template.HeaderColor);
        var headerTextColor = ParseColor(template.HeaderTextColor);

        container.Background(headerColor).Padding(20).Row(row =>
        {
            // Left side - Company info
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(invoice.CompanyName)
                    .FontSize(16).Bold().FontColor(headerTextColor);

                if (!string.IsNullOrEmpty(invoice.CompanyAddress1))
                    col.Item().Text(invoice.CompanyAddress1).FontSize(9).FontColor(headerTextColor);
                if (!string.IsNullOrEmpty(invoice.CompanyCity) || !string.IsNullOrEmpty(invoice.CompanyState))
                    col.Item().Text($"{invoice.CompanyCity}, {invoice.CompanyState} {invoice.CompanyPostalCode}".Trim(' ', ','))
                        .FontSize(9).FontColor(headerTextColor);
                if (!string.IsNullOrEmpty(invoice.CompanyPhone))
                    col.Item().Text($"Tel: {invoice.CompanyPhone}").FontSize(9).FontColor(headerTextColor);
            });

            // Right side - Ship To
            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("SHIP TO").FontSize(10).Bold().FontColor(headerTextColor).AlignRight();

                col.Item().Text(invoice.CustomerName)
                    .FontSize(12).Bold().FontColor(headerTextColor).AlignRight();

                var shippingAddress = DeserializeAddress(invoice.ShippingAddressJson) ?? DeserializeAddress(invoice.BillingAddressJson);
                if (shippingAddress != null)
                {
                    if (!string.IsNullOrEmpty(shippingAddress.Address1))
                        col.Item().Text(shippingAddress.Address1).FontSize(9).FontColor(headerTextColor).AlignRight();
                    if (!string.IsNullOrEmpty(shippingAddress.Address2))
                        col.Item().Text(shippingAddress.Address2).FontSize(9).FontColor(headerTextColor).AlignRight();
                    col.Item().Text($"{shippingAddress.City}, {shippingAddress.StateProvince} {shippingAddress.PostalCode}".Trim(' ', ','))
                        .FontSize(9).FontColor(headerTextColor).AlignRight();
                    if (!string.IsNullOrEmpty(shippingAddress.Country))
                        col.Item().Text(shippingAddress.Country).FontSize(9).FontColor(headerTextColor).AlignRight();
                }
                if (!string.IsNullOrEmpty(invoice.CustomerPhone))
                    col.Item().Text($"Tel: {invoice.CustomerPhone}").FontSize(9).FontColor(headerTextColor).AlignRight();
            });
        });
    }

    private void ComposePackingSlipContent(IContainer container, Invoice invoice, InvoiceTemplate template)
    {
        container.Padding(20).Column(col =>
        {
            // Packing slip title and order info
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(template.PackingSlipTitle.ToLower() + ".")
                    .FontSize(36).FontColor(Colors.Grey.Darken3);

                row.RelativeItem().AlignRight().Column(rightCol =>
                {
                    rightCol.Item().Text($"Order #: {invoice.Order?.OrderNumber ?? invoice.InvoiceNumber}")
                        .FontSize(10).AlignRight();
                    rightCol.Item().Text($"Date: {invoice.IssuedAt.ToString(template.DateFormat)}")
                        .FontSize(10).AlignRight();
                });
            });

            col.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Line items table (NO PRICES)
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // #
                    columns.RelativeColumn(4);   // Item Name
                    columns.ConstantColumn(80);  // SKU
                    columns.ConstantColumn(80);  // Quantity
                    columns.ConstantColumn(80);  // Packed (checkbox)
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Darken1).Padding(8)
                        .Text("#").Bold().FontSize(10);
                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Darken1).Padding(8)
                        .Text("Item Description").Bold().FontSize(10);
                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Darken1).Padding(8)
                        .Text("SKU").Bold().FontSize(10);
                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Darken1).Padding(8)
                        .Text("Qty").Bold().FontSize(10).AlignCenter();
                    header.Cell().BorderBottom(2).BorderColor(Colors.Grey.Darken1).Padding(8)
                        .Text("Packed").Bold().FontSize(10).AlignCenter();
                });

                // Line items
                var lineItems = DeserializeLineItems(invoice.LineItemsJson);
                var index = 1;
                foreach (var item in lineItems)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .Text(index.ToString()).FontSize(10);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .Column(c =>
                        {
                            c.Item().Text(item.ProductName).FontSize(10);
                            if (!string.IsNullOrEmpty(item.VariantName))
                                c.Item().Text(item.VariantName).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .Text(item.Sku ?? "-").FontSize(10);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .Text(item.Quantity.ToString()).FontSize(12).Bold().AlignCenter();
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                        .AlignCenter().AlignMiddle()
                        .Width(20).Height(20).Border(1).BorderColor(Colors.Grey.Darken1);

                    index++;
                }
            });

            // Total items
            col.Item().PaddingTop(15).AlignRight().Text($"Total Items: {DeserializeLineItems(invoice.LineItemsJson).Sum(x => x.Quantity)}")
                .FontSize(12).Bold();

            // Notes section
            col.Item().PaddingTop(30).Column(notesCol =>
            {
                notesCol.Item().Text("Packing Notes:").FontSize(10).Bold();
                notesCol.Item().PaddingTop(5).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).MinHeight(60)
                    .Text(invoice.Notes ?? "").FontSize(9);
            });

            // Verification section
            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Packed By:").FontSize(10).Bold();
                    c.Item().PaddingTop(20).Width(150).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                    c.Item().Text("Name / Date").FontSize(8).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Verified By:").FontSize(10).Bold();
                    c.Item().PaddingTop(20).Width(150).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                    c.Item().Text("Name / Date").FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private void ComposePackingSlipFooter(IContainer container, Invoice invoice, InvoiceTemplate template)
    {
        container.Padding(20).Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text("This is a packing slip. Prices are not shown.")
                    .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text("Generated by Algora Commerce")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    #region Helper Methods

    private static InvoiceTemplate GetDefaultTemplate() => new()
    {
        Name = "Default",
        Code = "DEFAULT",
        HeaderColor = "#1e3a5f",
        HeaderTextColor = "#ffffff",
        PrimaryColor = "#1e3a5f",
        AccentColor = "#2563eb",
        InvoiceTitle = "INVOICE",
        PackingSlipTitle = "PACKING SLIP",
        ShowAmountInWords = true,
        ShowHsnSacCodes = true,
        ShowQrCode = true,
        ThankYouMessage = "Thank you for your business!"
    };

    private static Color ParseColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Colors.Grey.Darken3;

        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            var r = Convert.ToByte(hex.Substring(0, 2), 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromRGB(r, g, b);
        }
        return Colors.Grey.Darken3;
    }

    private static string FormatCurrency(decimal amount, string currencyCode)
    {
        var symbol = currencyCode switch
        {
            "INR" => "\u20b9",
            "USD" => "$",
            "EUR" => "\u20ac",
            "GBP" => "\u00a3",
            _ => currencyCode + " "
        };
        return $"{symbol}{amount:N2}";
    }

    private static AddressData? DeserializeAddress(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<AddressData>(json);
        }
        catch
        {
            return null;
        }
    }

    private static List<InvoiceLineItem> DeserializeLineItems(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<InvoiceLineItem>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<InvoiceLineItem>>(json) ?? new List<InvoiceLineItem>();
        }
        catch
        {
            return new List<InvoiceLineItem>();
        }
    }

    public static string ConvertToWords(decimal amount, string currencyCode = "INR")
    {
        var ones = new[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        var tens = new[] { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        var wholePart = (long)Math.Floor(amount);
        var decimalPart = (int)((amount - wholePart) * 100);

        string ConvertGroup(long n)
        {
            if (n < 20) return ones[n];
            if (n < 100) return tens[n / 10] + (n % 10 > 0 ? " " + ones[n % 10] : "");
            return ones[n / 100] + " Hundred" + (n % 100 > 0 ? " " + ConvertGroup(n % 100) : "");
        }

        var words = "";
        if (wholePart >= 10000000) { words += ConvertGroup(wholePart / 10000000) + " Crore "; wholePart %= 10000000; }
        if (wholePart >= 100000) { words += ConvertGroup(wholePart / 100000) + " Lakh "; wholePart %= 100000; }
        if (wholePart >= 1000) { words += ConvertGroup(wholePart / 1000) + " Thousand "; wholePart %= 1000; }
        if (wholePart > 0) words += ConvertGroup(wholePart);

        var currencyName = currencyCode switch
        {
            "INR" => "Rupees",
            "USD" => "Dollars",
            "EUR" => "Euros",
            "GBP" => "Pounds",
            _ => currencyCode
        };

        var result = $"{words.Trim()} {currencyName}";
        if (decimalPart > 0)
        {
            var paisaName = currencyCode == "INR" ? "Paise" : "Cents";
            result += $" and {ConvertGroup(decimalPart)} {paisaName}";
        }
        result += " Only";

        return result;
    }

    #endregion

    #region Helper Classes

    private class AddressData
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Company { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
    }

    private class InvoiceLineItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantName { get; set; }
        public string? Sku { get; set; }
        public string? HsnCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    #endregion
}
