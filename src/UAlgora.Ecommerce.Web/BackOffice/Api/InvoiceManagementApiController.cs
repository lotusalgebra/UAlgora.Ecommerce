using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UAlgora.Ecommerce.Core.Constants;
using UAlgora.Ecommerce.Core.Interfaces.Repositories;
using UAlgora.Ecommerce.Core.Interfaces.Services;
using UAlgora.Ecommerce.Core.Models.Domain;
using UAlgora.Ecommerce.Web.Services;
using Umbraco.Cms.Api.Management.Routing;

namespace UAlgora.Ecommerce.Web.BackOffice.Api;

/// <summary>
/// Management API controller for invoice operations.
/// </summary>
[VersionedApiBackOfficeRoute($"{EcommerceConstants.ApiRouteBase}/invoice")]
public class InvoiceManagementApiController : EcommerceManagementApiControllerBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOrderService _orderService;
    private readonly IInvoicePdfService _invoicePdfService;

    public InvoiceManagementApiController(
        IInvoiceRepository invoiceRepository,
        IOrderService orderService,
        IInvoicePdfService invoicePdfService)
    {
        _invoiceRepository = invoiceRepository;
        _orderService = orderService;
        _invoicePdfService = invoicePdfService;
    }

    #region Invoices

    /// <summary>
    /// Gets all invoices for an order.
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType<InvoiceListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken ct = default)
    {
        var invoices = await _invoiceRepository.GetByOrderIdAsync(orderId, ct);
        return Ok(new InvoiceListResponse
        {
            Items = invoices.Select(MapToModel).ToList(),
            Total = invoices.Count
        });
    }

    /// <summary>
    /// Gets an invoice by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvoiceModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct);
        if (invoice == null)
            return NotFound();

        return Ok(MapToModel(invoice));
    }

    /// <summary>
    /// Generates an invoice for an order.
    /// </summary>
    [HttpPost("generate/{orderId:guid}")]
    [ProducesResponseType<InvoiceModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateInvoice(Guid orderId, [FromBody] GenerateInvoiceRequest? request = null, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, ct);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        // Get template
        InvoiceTemplate? template = null;
        if (request?.TemplateId.HasValue == true)
        {
            template = await _invoiceRepository.GetTemplateByIdAsync(request.TemplateId.Value, ct);
        }
        template ??= await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.Invoice, ct);

        // Generate invoice number
        var prefix = template?.InvoiceNumberPrefix ?? "INV-";
        var includeYear = template?.IncludeYearInNumber ?? true;
        var padding = template?.NumberPadding ?? 6;
        var invoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(prefix, includeYear, padding, ct);

        // Create invoice
        var invoice = new Invoice
        {
            OrderId = orderId,
            StoreId = order.StoreId,
            InvoiceNumber = invoiceNumber,
            Status = InvoiceStatus.Draft,
            IssuedAt = DateTime.UtcNow,
            DueAt = DateTime.UtcNow.AddDays(template?.PaymentTermsDays ?? 30),
            TemplateId = template?.Id,

            // Company info from template or request
            CompanyName = request?.CompanyName ?? template?.CompanyName ?? "Your Company Name",
            CompanyAddress1 = request?.CompanyAddress ?? template?.CompanyAddress,
            CompanyPhone = request?.CompanyPhone ?? template?.CompanyPhone,
            CompanyEmail = request?.CompanyEmail ?? template?.CompanyEmail,
            CompanyWebsite = request?.CompanyWebsite ?? template?.CompanyWebsite,
            TaxId = request?.TaxId ?? template?.TaxId,
            LogoUrl = request?.LogoUrl ?? template?.LogoUrl,

            // Customer info from order
            CustomerName = order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim(),
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            BillingAddressJson = order.BillingAddress != null ? JsonSerializer.Serialize(MapAddressToModel(order.BillingAddress)) : null,
            ShippingAddressJson = order.ShippingAddress != null ? JsonSerializer.Serialize(MapAddressToModel(order.ShippingAddress)) : null,

            // Amounts
            CurrencyCode = order.CurrencyCode,
            Subtotal = order.Subtotal,
            DiscountTotal = order.DiscountTotal,
            ShippingTotal = order.ShippingTotal,
            TaxTotal = order.TaxTotal,
            GrandTotal = order.GrandTotal,
            PaidAmount = order.PaidAmount,

            // Line items
            LineItemsJson = JsonSerializer.Serialize(order.Lines.Select(l => new InvoiceLineItem
            {
                ProductName = l.ProductName,
                Sku = l.Sku,
                VariantName = l.VariantName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountAmount = l.DiscountAmount,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                ImageUrl = l.ImageUrl
            }).ToList()),

            // Text
            Notes = request?.Notes ?? template?.DefaultNotes,
            Terms = request?.Terms ?? template?.DefaultTerms,
            Footer = request?.Footer ?? template?.DefaultFooter,
            PaymentInstructions = request?.PaymentInstructions ?? template?.DefaultPaymentInstructions,

            // GST fields
            IsGstApplicable = request?.IsGstApplicable ?? false,
            CgstRate = request?.CgstRate ?? 0,
            SgstRate = request?.SgstRate ?? 0,
            IgstRate = request?.IgstRate ?? 0,
            CompanyGstin = request?.CompanyGstin,
            CustomerGstin = request?.CustomerGstin,
            PlaceOfSupply = request?.PlaceOfSupply,
            IsInterState = request?.IsInterState ?? false
        };

        // Calculate GST amounts if GST is applicable
        if (invoice.IsGstApplicable)
        {
            var taxableAmount = invoice.Subtotal - invoice.DiscountTotal;
            if (invoice.IsInterState)
            {
                invoice.IgstAmount = Math.Round(taxableAmount * invoice.IgstRate / 100, 2);
                invoice.CgstAmount = 0;
                invoice.SgstAmount = 0;
            }
            else
            {
                invoice.CgstAmount = Math.Round(taxableAmount * invoice.CgstRate / 100, 2);
                invoice.SgstAmount = Math.Round(taxableAmount * invoice.SgstRate / 100, 2);
                invoice.IgstAmount = 0;
            }
            // Update tax total to match GST calculation
            invoice.TaxTotal = invoice.CgstAmount + invoice.SgstAmount + invoice.IgstAmount;
            invoice.GrandTotal = invoice.Subtotal - invoice.DiscountTotal + invoice.ShippingTotal + invoice.TaxTotal;
        }

        var created = await _invoiceRepository.AddAsync(invoice, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToModel(created));
    }

    /// <summary>
    /// Gets invoice HTML for printing/PDF.
    /// </summary>
    [HttpGet("{id:guid}/html")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceHtml(Guid id, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct);
        if (invoice == null)
            return NotFound();

        var template = invoice.TemplateId.HasValue
            ? await _invoiceRepository.GetTemplateByIdAsync(invoice.TemplateId.Value, ct)
            : await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.Invoice, ct);

        var html = GenerateInvoiceHtml(invoice, template);
        return Content(html, "text/html");
    }

    /// <summary>
    /// Gets invoice HTML directly from order for printing.
    /// </summary>
    [HttpGet("order/{orderId:guid}/html")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderInvoiceHtml(Guid orderId, [FromQuery] Guid? templateId = null, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, ct);
        if (order == null)
            return NotFound();

        var template = templateId.HasValue
            ? await _invoiceRepository.GetTemplateByIdAsync(templateId.Value, ct)
            : await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.Invoice, ct);

        var html = GenerateOrderInvoiceHtml(order, template);
        return Content(html, "text/html");
    }

    /// <summary>
    /// Gets packaging slip HTML for printing.
    /// </summary>
    [HttpGet("order/{orderId:guid}/packing-slip")]
    [Produces("text/html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPackingSlipHtml(Guid orderId, [FromQuery] Guid? templateId = null, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, ct);
        if (order == null)
            return NotFound();

        var template = templateId.HasValue
            ? await _invoiceRepository.GetTemplateByIdAsync(templateId.Value, ct)
            : await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.PackagingSlip, ct);

        var html = GeneratePackingSlipHtml(order, template);
        return Content(html, "text/html");
    }

    #endregion

    #region PDF Generation

    /// <summary>
    /// Downloads invoice as PDF.
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoicePdf(Guid id, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct);
        if (invoice == null)
            return NotFound();

        var template = invoice.TemplateId.HasValue
            ? await _invoiceRepository.GetTemplateByIdAsync(invoice.TemplateId.Value, ct)
            : await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.Invoice, ct);

        // Set amount in words if not already set
        if (string.IsNullOrEmpty(invoice.AmountInWords))
        {
            invoice.AmountInWords = InvoicePdfService.ConvertToWords(invoice.GrandTotal, invoice.CurrencyCode);
        }

        var pdfBytes = _invoicePdfService.GenerateInvoicePdf(invoice, template);
        return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
    }

    /// <summary>
    /// Generates invoice PDF directly from an order.
    /// </summary>
    [HttpGet("order/{orderId:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateOrderInvoicePdf(Guid orderId, [FromQuery] Guid? templateId = null, [FromQuery] string? taxSystem = null, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, ct);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        var template = templateId.HasValue
            ? await _invoiceRepository.GetTemplateByIdAsync(templateId.Value, ct)
            : await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.Invoice, ct);

        // Create a temporary invoice object for PDF generation
        var invoice = CreateInvoiceFromOrder(order, template, taxSystem);

        var pdfBytes = _invoicePdfService.GenerateInvoicePdf(invoice, template);
        return File(pdfBytes, "application/pdf", $"Invoice-{order.OrderNumber}.pdf");
    }

    /// <summary>
    /// Downloads packing slip as PDF for an order.
    /// </summary>
    [HttpGet("order/{orderId:guid}/packing-slip/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadPackingSlipPdf(Guid orderId, [FromQuery] Guid? templateId = null, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(orderId, ct);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        var template = templateId.HasValue
            ? await _invoiceRepository.GetTemplateByIdAsync(templateId.Value, ct)
            : await _invoiceRepository.GetDefaultTemplateAsync(InvoiceTemplateType.PackagingSlip, ct);

        // Create a temporary invoice object for packing slip generation
        var invoice = CreateInvoiceFromOrder(order, template, null);

        var pdfBytes = _invoicePdfService.GeneratePackingSlipPdf(invoice, template);
        return File(pdfBytes, "application/pdf", $"PackingSlip-{order.OrderNumber}.pdf");
    }

    /// <summary>
    /// Creates an Invoice object from an Order for PDF generation.
    /// </summary>
    private Invoice CreateInvoiceFromOrder(Order order, InvoiceTemplate? template, string? taxSystemOverride)
    {
        // Determine tax system
        var taxSystem = TaxSystemType.None;
        if (!string.IsNullOrEmpty(taxSystemOverride))
        {
            Enum.TryParse<TaxSystemType>(taxSystemOverride, true, out taxSystem);
        }
        else if (order.CurrencyCode == "INR")
        {
            taxSystem = TaxSystemType.GST;
        }
        else if (order.CurrencyCode == "EUR" || order.CurrencyCode == "GBP")
        {
            taxSystem = TaxSystemType.VAT;
        }
        else if (order.CurrencyCode == "USD")
        {
            taxSystem = TaxSystemType.SalesTax;
        }

        var invoice = new Invoice
        {
            OrderId = order.Id,
            Order = order,
            StoreId = order.StoreId,
            InvoiceNumber = order.OrderNumber,
            Status = InvoiceStatus.Draft,
            IssuedAt = order.CreatedAt,
            TemplateId = template?.Id,

            // Tax system
            TaxSystem = taxSystem,
            TaxLabel = taxSystem switch
            {
                TaxSystemType.GST => "GST",
                TaxSystemType.VAT => "VAT",
                TaxSystemType.SalesTax => "Sales Tax",
                _ => "Tax"
            },

            // Company info from template
            CompanyName = template?.CompanyName ?? "Your Company Name",
            CompanyAddress1 = template?.CompanyAddress,
            CompanyPhone = template?.CompanyPhone,
            CompanyEmail = template?.CompanyEmail,
            CompanyWebsite = template?.CompanyWebsite,
            TaxId = template?.TaxId,
            LogoUrl = template?.LogoUrl,

            // GST fields from template
            CompanyGstin = template?.CompanyGstin,
            PlaceOfSupply = template?.DefaultPlaceOfSupply,
            SupplyTypeCode = template?.SupplyTypeCode,
            DocumentTypeCode = template?.DocumentTypeCode,

            // Customer info from order
            CustomerName = order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim(),
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            BillingAddressJson = order.BillingAddress != null ? JsonSerializer.Serialize(new
            {
                FirstName = order.BillingAddress.FirstName,
                LastName = order.BillingAddress.LastName,
                Company = order.BillingAddress.Company,
                Address1 = order.BillingAddress.AddressLine1,
                Address2 = order.BillingAddress.AddressLine2,
                City = order.BillingAddress.City,
                StateProvince = order.BillingAddress.StateProvince,
                PostalCode = order.BillingAddress.PostalCode,
                Country = order.BillingAddress.Country,
                Phone = order.BillingAddress.Phone
            }) : null,
            ShippingAddressJson = order.ShippingAddress != null ? JsonSerializer.Serialize(new
            {
                FirstName = order.ShippingAddress.FirstName,
                LastName = order.ShippingAddress.LastName,
                Company = order.ShippingAddress.Company,
                Address1 = order.ShippingAddress.AddressLine1,
                Address2 = order.ShippingAddress.AddressLine2,
                City = order.ShippingAddress.City,
                StateProvince = order.ShippingAddress.StateProvince,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country,
                Phone = order.ShippingAddress.Phone
            }) : null,

            // Amounts
            CurrencyCode = order.CurrencyCode,
            Subtotal = order.Subtotal,
            DiscountTotal = order.DiscountTotal,
            ShippingTotal = order.ShippingTotal,
            TaxTotal = order.TaxTotal,
            GrandTotal = order.GrandTotal,
            PaidAmount = order.PaidAmount,

            // Line items
            LineItemsJson = JsonSerializer.Serialize(order.Lines.Select(l => new
            {
                ProductId = l.ProductId,
                ProductName = l.ProductName,
                VariantName = l.VariantName,
                Sku = l.Sku,
                HsnCode = (string?)null, // Would come from product if available
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxRate = order.TaxTotal > 0 && order.Subtotal > 0 ? Math.Round(order.TaxTotal / order.Subtotal * 100, 2) : 0,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal
            }).ToList()),

            // Text from template
            Notes = template?.DefaultNotes,
            Terms = template?.DefaultTerms,
            Footer = template?.DefaultFooter,
            PaymentInstructions = template?.DefaultPaymentInstructions
        };

        // Set GST/VAT amounts based on tax system
        if (taxSystem == TaxSystemType.GST)
        {
            invoice.IsGstApplicable = true;
            // Assume intra-state by default (CGST + SGST)
            var taxRate = order.TaxTotal > 0 && order.Subtotal > 0 ? Math.Round(order.TaxTotal / order.Subtotal * 100, 2) : 18;
            invoice.CgstRate = taxRate / 2;
            invoice.SgstRate = taxRate / 2;
            invoice.CgstAmount = Math.Round(order.TaxTotal / 2, 2);
            invoice.SgstAmount = order.TaxTotal - invoice.CgstAmount;
        }
        else if (taxSystem == TaxSystemType.VAT)
        {
            invoice.VatRate = order.TaxTotal > 0 && order.Subtotal > 0 ? Math.Round(order.TaxTotal / order.Subtotal * 100, 2) : 20;
            invoice.VatAmount = order.TaxTotal;
        }

        // Set amount in words
        invoice.AmountInWords = InvoicePdfService.ConvertToWords(invoice.GrandTotal, invoice.CurrencyCode);

        return invoice;
    }

    /// <summary>
    /// Updates invoice status.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<InvoiceModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken ct = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, ct);
        if (invoice == null)
            return NotFound();

        if (!Enum.TryParse<InvoiceStatus>(request.Status, true, out var status))
            return BadRequest(new { message = "Invalid status" });

        invoice.Status = status;

        if (status == InvoiceStatus.Paid && !invoice.PaidAt.HasValue)
            invoice.PaidAt = DateTime.UtcNow;
        else if (status == InvoiceStatus.Voided && !invoice.VoidedAt.HasValue)
            invoice.VoidedAt = DateTime.UtcNow;

        var updated = await _invoiceRepository.UpdateAsync(invoice, ct);
        return Ok(MapToModel(updated));
    }

    #endregion

    #region Templates

    /// <summary>
    /// Gets all invoice templates.
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType<TemplateListResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(CancellationToken ct = default)
    {
        var templates = await _invoiceRepository.GetTemplatesAsync(ct);
        return Ok(new TemplateListResponse
        {
            Items = templates.Select(MapTemplateToModel).ToList(),
            Total = templates.Count
        });
    }

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    [HttpGet("templates/{id:guid}")]
    [ProducesResponseType<InvoiceTemplateModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(Guid id, CancellationToken ct = default)
    {
        var template = await _invoiceRepository.GetTemplateByIdAsync(id, ct);
        if (template == null)
            return NotFound();

        return Ok(MapTemplateToModel(template));
    }

    /// <summary>
    /// Creates a new template.
    /// </summary>
    [HttpPost("templates")]
    [ProducesResponseType<InvoiceTemplateModel>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request, CancellationToken ct = default)
    {
        var template = new InvoiceTemplate
        {
            Name = request.Name,
            Code = request.Code ?? request.Name.ToLowerInvariant().Replace(" ", "-"),
            Description = request.Description,
            IsDefault = request.IsDefault,
            TemplateType = Enum.TryParse<InvoiceTemplateType>(request.TemplateType, true, out var type) ? type : InvoiceTemplateType.Invoice,
            CompanyName = request.CompanyName,
            CompanyAddress = request.CompanyAddress,
            CompanyPhone = request.CompanyPhone,
            CompanyEmail = request.CompanyEmail,
            CompanyWebsite = request.CompanyWebsite,
            TaxId = request.TaxId,
            LogoUrl = request.LogoUrl,
            PrimaryColor = request.PrimaryColor ?? "#1976d2",
            SecondaryColor = request.SecondaryColor ?? "#424242",
            AccentColor = request.AccentColor ?? "#ff5722",
            FontFamily = request.FontFamily ?? "Arial, sans-serif",
            CustomCss = request.CustomCss,
            ShowLogo = request.ShowLogo,
            ShowShippingAddress = request.ShowShippingAddress,
            ShowProductImages = request.ShowProductImages,
            ShowSku = request.ShowSku,
            ShowTaxBreakdown = request.ShowTaxBreakdown,
            ShowPaymentInstructions = request.ShowPaymentInstructions,
            DefaultNotes = request.DefaultNotes,
            DefaultTerms = request.DefaultTerms,
            DefaultFooter = request.DefaultFooter,
            DefaultPaymentInstructions = request.DefaultPaymentInstructions,
            InvoiceTitle = request.InvoiceTitle ?? "INVOICE",
            PackingSlipTitle = request.PackingSlipTitle ?? "PACKING SLIP",
            InvoiceNumberPrefix = request.InvoiceNumberPrefix ?? "INV-",
            IncludeYearInNumber = request.IncludeYearInNumber,
            NumberPadding = request.NumberPadding,
            DateFormat = request.DateFormat ?? "MMM dd, yyyy",
            PaymentTermsDays = request.PaymentTermsDays
        };

        var created = await _invoiceRepository.AddTemplateAsync(template, ct);
        return CreatedAtAction(nameof(GetTemplate), new { id = created.Id }, MapTemplateToModel(created));
    }

    /// <summary>
    /// Updates a template.
    /// </summary>
    [HttpPut("templates/{id:guid}")]
    [ProducesResponseType<InvoiceTemplateModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request, CancellationToken ct = default)
    {
        var template = await _invoiceRepository.GetTemplateByIdAsync(id, ct);
        if (template == null)
            return NotFound();

        if (request.Name != null) template.Name = request.Name;
        if (request.Code != null) template.Code = request.Code;
        if (request.Description != null) template.Description = request.Description;
        if (request.IsDefault.HasValue) template.IsDefault = request.IsDefault.Value;
        if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;
        if (request.CompanyName != null) template.CompanyName = request.CompanyName;
        if (request.CompanyAddress != null) template.CompanyAddress = request.CompanyAddress;
        if (request.CompanyPhone != null) template.CompanyPhone = request.CompanyPhone;
        if (request.CompanyEmail != null) template.CompanyEmail = request.CompanyEmail;
        if (request.CompanyWebsite != null) template.CompanyWebsite = request.CompanyWebsite;
        if (request.TaxId != null) template.TaxId = request.TaxId;
        if (request.LogoUrl != null) template.LogoUrl = request.LogoUrl;
        if (request.PrimaryColor != null) template.PrimaryColor = request.PrimaryColor;
        if (request.SecondaryColor != null) template.SecondaryColor = request.SecondaryColor;
        if (request.AccentColor != null) template.AccentColor = request.AccentColor;
        if (request.FontFamily != null) template.FontFamily = request.FontFamily;
        if (request.CustomCss != null) template.CustomCss = request.CustomCss;
        if (request.ShowLogo.HasValue) template.ShowLogo = request.ShowLogo.Value;
        if (request.ShowShippingAddress.HasValue) template.ShowShippingAddress = request.ShowShippingAddress.Value;
        if (request.ShowProductImages.HasValue) template.ShowProductImages = request.ShowProductImages.Value;
        if (request.ShowSku.HasValue) template.ShowSku = request.ShowSku.Value;
        if (request.ShowTaxBreakdown.HasValue) template.ShowTaxBreakdown = request.ShowTaxBreakdown.Value;
        if (request.ShowPaymentInstructions.HasValue) template.ShowPaymentInstructions = request.ShowPaymentInstructions.Value;
        if (request.DefaultNotes != null) template.DefaultNotes = request.DefaultNotes;
        if (request.DefaultTerms != null) template.DefaultTerms = request.DefaultTerms;
        if (request.DefaultFooter != null) template.DefaultFooter = request.DefaultFooter;
        if (request.DefaultPaymentInstructions != null) template.DefaultPaymentInstructions = request.DefaultPaymentInstructions;
        if (request.InvoiceTitle != null) template.InvoiceTitle = request.InvoiceTitle;
        if (request.PackingSlipTitle != null) template.PackingSlipTitle = request.PackingSlipTitle;
        if (request.InvoiceNumberPrefix != null) template.InvoiceNumberPrefix = request.InvoiceNumberPrefix;
        if (request.IncludeYearInNumber.HasValue) template.IncludeYearInNumber = request.IncludeYearInNumber.Value;
        if (request.NumberPadding.HasValue) template.NumberPadding = request.NumberPadding.Value;
        if (request.DateFormat != null) template.DateFormat = request.DateFormat;
        if (request.PaymentTermsDays.HasValue) template.PaymentTermsDays = request.PaymentTermsDays.Value;

        var updated = await _invoiceRepository.UpdateTemplateAsync(template, ct);
        return Ok(MapTemplateToModel(updated));
    }

    /// <summary>
    /// Deletes a template.
    /// </summary>
    [HttpDelete("templates/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken ct = default)
    {
        var template = await _invoiceRepository.GetTemplateByIdAsync(id, ct);
        if (template == null)
            return NotFound();

        await _invoiceRepository.DeleteTemplateAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    /// Seeds default templates.
    /// </summary>
    [HttpPost("templates/seed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedTemplates(CancellationToken ct = default)
    {
        var existing = await _invoiceRepository.GetTemplatesAsync(ct);
        if (existing.Count > 0)
            return Ok(new { message = "Templates already exist", count = existing.Count });

        var templates = new List<InvoiceTemplate>
        {
            new()
            {
                Name = "Standard Invoice",
                Code = "standard-invoice",
                Description = "Clean, professional invoice template",
                IsDefault = true,
                TemplateType = InvoiceTemplateType.Invoice,
                PrimaryColor = "#1976d2",
                InvoiceTitle = "INVOICE",
                DefaultFooter = "Thank you for your business!",
                DefaultTerms = "Payment is due within 30 days of invoice date."
            },
            new()
            {
                Name = "Standard Packing Slip",
                Code = "standard-packing-slip",
                Description = "Clean packing slip for shipments",
                IsDefault = true,
                TemplateType = InvoiceTemplateType.PackagingSlip,
                PrimaryColor = "#424242",
                PackingSlipTitle = "PACKING SLIP",
                ShowTaxBreakdown = false,
                ShowPaymentInstructions = false
            }
        };

        foreach (var template in templates)
        {
            await _invoiceRepository.AddTemplateAsync(template, ct);
        }

        return Ok(new { message = "Default templates created", count = templates.Count });
    }

    #endregion

    #region HTML Generation

    private string GenerateInvoiceHtml(Invoice invoice, InvoiceTemplate? template)
    {
        var lineItems = !string.IsNullOrEmpty(invoice.LineItemsJson)
            ? JsonSerializer.Deserialize<List<InvoiceLineItem>>(invoice.LineItemsJson) ?? []
            : [];

        var billingAddress = !string.IsNullOrEmpty(invoice.BillingAddressJson)
            ? JsonSerializer.Deserialize<InvoiceAddressModel>(invoice.BillingAddressJson)
            : null;

        var shippingAddress = !string.IsNullOrEmpty(invoice.ShippingAddressJson)
            ? JsonSerializer.Deserialize<InvoiceAddressModel>(invoice.ShippingAddressJson)
            : null;

        return GenerateDocumentHtml(
            title: template?.InvoiceTitle ?? "INVOICE",
            documentNumber: invoice.InvoiceNumber,
            documentDate: invoice.IssuedAt,
            dueDate: invoice.DueAt,
            companyName: invoice.CompanyName,
            companyAddress: invoice.CompanyAddress1,
            companyPhone: invoice.CompanyPhone,
            companyEmail: invoice.CompanyEmail,
            companyWebsite: invoice.CompanyWebsite,
            taxId: invoice.TaxId,
            logoUrl: invoice.LogoUrl,
            customerName: invoice.CustomerName,
            customerEmail: invoice.CustomerEmail,
            billingAddress: billingAddress,
            shippingAddress: shippingAddress,
            lineItems: lineItems,
            subtotal: invoice.Subtotal,
            discountTotal: invoice.DiscountTotal,
            shippingTotal: invoice.ShippingTotal,
            taxTotal: invoice.TaxTotal,
            grandTotal: invoice.GrandTotal,
            paidAmount: invoice.PaidAmount,
            currencyCode: invoice.CurrencyCode,
            notes: invoice.Notes,
            terms: invoice.Terms,
            footer: invoice.Footer,
            paymentInstructions: invoice.PaymentInstructions,
            template: template,
            isPackingSlip: false,
            // GST parameters
            isGstApplicable: invoice.IsGstApplicable,
            cgstAmount: invoice.CgstAmount,
            sgstAmount: invoice.SgstAmount,
            igstAmount: invoice.IgstAmount,
            cgstRate: invoice.CgstRate,
            sgstRate: invoice.SgstRate,
            igstRate: invoice.IgstRate,
            companyGstin: invoice.CompanyGstin,
            customerGstin: invoice.CustomerGstin,
            placeOfSupply: invoice.PlaceOfSupply,
            isInterState: invoice.IsInterState
        );
    }

    private string GenerateOrderInvoiceHtml(Order order, InvoiceTemplate? template)
    {
        var lineItems = order.Lines.Select(l => new InvoiceLineItem
        {
            ProductName = l.ProductName,
            Sku = l.Sku,
            VariantName = l.VariantName,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            DiscountAmount = l.DiscountAmount,
            TaxAmount = l.TaxAmount,
            LineTotal = l.LineTotal,
            ImageUrl = l.ImageUrl
        }).ToList();

        return GenerateDocumentHtml(
            title: template?.InvoiceTitle ?? "INVOICE",
            documentNumber: order.OrderNumber,
            documentDate: order.CreatedAt,
            dueDate: null,
            companyName: template?.CompanyName ?? "Your Company",
            companyAddress: template?.CompanyAddress,
            companyPhone: template?.CompanyPhone,
            companyEmail: template?.CompanyEmail,
            companyWebsite: template?.CompanyWebsite,
            taxId: template?.TaxId,
            logoUrl: template?.LogoUrl,
            customerName: order.CustomerName ?? $"{order.BillingAddress?.FirstName} {order.BillingAddress?.LastName}".Trim(),
            customerEmail: order.CustomerEmail,
            billingAddress: order.BillingAddress != null ? MapAddressToModel(order.BillingAddress) : null,
            shippingAddress: order.ShippingAddress != null ? MapAddressToModel(order.ShippingAddress) : null,
            lineItems: lineItems,
            subtotal: order.Subtotal,
            discountTotal: order.DiscountTotal,
            shippingTotal: order.ShippingTotal,
            taxTotal: order.TaxTotal,
            grandTotal: order.GrandTotal,
            paidAmount: order.PaidAmount,
            currencyCode: order.CurrencyCode,
            notes: template?.DefaultNotes,
            terms: template?.DefaultTerms,
            footer: template?.DefaultFooter,
            paymentInstructions: template?.DefaultPaymentInstructions,
            template: template,
            isPackingSlip: false
        );
    }

    private string GeneratePackingSlipHtml(Order order, InvoiceTemplate? template)
    {
        var lineItems = order.Lines.Select(l => new InvoiceLineItem
        {
            ProductName = l.ProductName,
            Sku = l.Sku,
            VariantName = l.VariantName,
            Quantity = l.Quantity,
            UnitPrice = 0,
            LineTotal = 0,
            ImageUrl = l.ImageUrl
        }).ToList();

        return GenerateDocumentHtml(
            title: template?.PackingSlipTitle ?? "PACKING SLIP",
            documentNumber: order.OrderNumber,
            documentDate: order.CreatedAt,
            dueDate: null,
            companyName: template?.CompanyName ?? "Your Company",
            companyAddress: template?.CompanyAddress,
            companyPhone: template?.CompanyPhone,
            companyEmail: template?.CompanyEmail,
            companyWebsite: template?.CompanyWebsite,
            taxId: null,
            logoUrl: template?.LogoUrl,
            customerName: order.CustomerName ?? $"{order.ShippingAddress?.FirstName} {order.ShippingAddress?.LastName}".Trim(),
            customerEmail: order.CustomerEmail,
            billingAddress: null,
            shippingAddress: order.ShippingAddress != null ? MapAddressToModel(order.ShippingAddress) : null,
            lineItems: lineItems,
            subtotal: 0,
            discountTotal: 0,
            shippingTotal: 0,
            taxTotal: 0,
            grandTotal: 0,
            paidAmount: 0,
            currencyCode: order.CurrencyCode,
            notes: order.CustomerNote,
            terms: null,
            footer: template?.DefaultFooter,
            paymentInstructions: null,
            template: template,
            isPackingSlip: true
        );
    }

    private static string GenerateDocumentHtml(
        string title, string documentNumber, DateTime documentDate, DateTime? dueDate,
        string companyName, string? companyAddress, string? companyPhone, string? companyEmail, string? companyWebsite, string? taxId, string? logoUrl,
        string customerName, string? customerEmail, InvoiceAddressModel? billingAddress, InvoiceAddressModel? shippingAddress,
        List<InvoiceLineItem> lineItems,
        decimal subtotal, decimal discountTotal, decimal shippingTotal, decimal taxTotal, decimal grandTotal, decimal paidAmount,
        string currencyCode, string? notes, string? terms, string? footer, string? paymentInstructions,
        InvoiceTemplate? template, bool isPackingSlip,
        // GST parameters
        bool isGstApplicable = false, decimal cgstAmount = 0, decimal sgstAmount = 0, decimal igstAmount = 0,
        decimal cgstRate = 0, decimal sgstRate = 0, decimal igstRate = 0,
        string? companyGstin = null, string? customerGstin = null, string? placeOfSupply = null, bool isInterState = false)
    {
        var primaryColor = template?.PrimaryColor ?? "#1976d2";
        var secondaryColor = template?.SecondaryColor ?? "#424242";
        var fontFamily = template?.FontFamily ?? "Arial, sans-serif";
        var showSku = template?.ShowSku ?? true;
        var showShippingAddress = template?.ShowShippingAddress ?? true;
        var showTaxBreakdown = !isPackingSlip && (template?.ShowTaxBreakdown ?? true);
        var showPaymentInstructions = !isPackingSlip && (template?.ShowPaymentInstructions ?? true);
        var showLogo = template?.ShowLogo ?? true;

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>{title} - {documentNumber}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: {fontFamily}; font-size: 12px; color: #333; line-height: 1.5; padding: 40px; max-width: 800px; margin: 0 auto; }}
        .header {{ display: flex; justify-content: space-between; margin-bottom: 40px; }}
        .company-info {{ max-width: 300px; }}
        .company-name {{ font-size: 24px; font-weight: bold; color: {primaryColor}; margin-bottom: 8px; }}
        .company-details {{ font-size: 11px; color: #666; }}
        .logo {{ max-height: 80px; max-width: 200px; }}
        .document-info {{ text-align: right; }}
        .document-title {{ font-size: 32px; font-weight: bold; color: {primaryColor}; margin-bottom: 16px; text-transform: uppercase; }}
        .document-meta {{ font-size: 12px; }}
        .document-meta strong {{ color: {secondaryColor}; }}
        .addresses {{ display: flex; gap: 40px; margin-bottom: 30px; }}
        .address-block {{ flex: 1; }}
        .address-title {{ font-weight: bold; color: {primaryColor}; margin-bottom: 8px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; }}
        .address-content {{ background: #f8f9fa; padding: 15px; border-radius: 4px; }}
        table {{ width: 100%; border-collapse: collapse; margin-bottom: 30px; }}
        th {{ background: {primaryColor}; color: white; padding: 12px; text-align: left; font-size: 11px; text-transform: uppercase; }}
        td {{ padding: 12px; border-bottom: 1px solid #eee; }}
        .text-right {{ text-align: right; }}
        .text-center {{ text-align: center; }}
        .totals {{ width: 300px; margin-left: auto; }}
        .totals td {{ padding: 8px 12px; }}
        .totals .label {{ color: #666; }}
        .totals .grand-total {{ font-size: 16px; font-weight: bold; color: {primaryColor}; border-top: 2px solid {primaryColor}; }}
        .notes {{ margin-top: 30px; padding: 20px; background: #f8f9fa; border-radius: 4px; }}
        .notes-title {{ font-weight: bold; color: {primaryColor}; margin-bottom: 8px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #666; font-size: 11px; padding-top: 20px; border-top: 1px solid #eee; }}
        .payment-info {{ margin-top: 20px; padding: 15px; background: #fff3cd; border-radius: 4px; }}
        .payment-title {{ font-weight: bold; margin-bottom: 8px; }}
        @media print {{
            body {{ padding: 20px; }}
            .no-print {{ display: none; }}
        }}
        {template?.CustomCss ?? ""}
    </style>
</head>
<body>
    <div class=""header"">
        <div class=""company-info"">
            {(showLogo && !string.IsNullOrEmpty(logoUrl) ? $@"<img src=""{logoUrl}"" class=""logo"" />" : "")}
            <div class=""company-name"">{companyName}</div>
            <div class=""company-details"">
                {(!string.IsNullOrEmpty(companyAddress) ? $"{companyAddress}<br>" : "")}
                {(!string.IsNullOrEmpty(companyPhone) ? $"Phone: {companyPhone}<br>" : "")}
                {(!string.IsNullOrEmpty(companyEmail) ? $"Email: {companyEmail}<br>" : "")}
                {(!string.IsNullOrEmpty(companyWebsite) ? $"Web: {companyWebsite}<br>" : "")}
                {(!string.IsNullOrEmpty(taxId) ? $"Tax ID: {taxId}<br>" : "")}
                {(!string.IsNullOrEmpty(companyGstin) ? $"GSTIN: {companyGstin}" : "")}
            </div>
        </div>
        <div class=""document-info"">
            <div class=""document-title"">{title}</div>
            <div class=""document-meta"">
                <strong>{(isPackingSlip ? "Order" : "Invoice")} #:</strong> {documentNumber}<br>
                <strong>Date:</strong> {documentDate:MMM dd, yyyy}<br>
                {(dueDate.HasValue && !isPackingSlip ? $"<strong>Due Date:</strong> {dueDate:MMM dd, yyyy}" : "")}
            </div>
        </div>
    </div>

    <div class=""addresses"">
        {(billingAddress != null && !isPackingSlip ? $@"
        <div class=""address-block"">
            <div class=""address-title"">Bill To</div>
            <div class=""address-content"">
                <strong>{billingAddress.FirstName} {billingAddress.LastName}</strong><br>
                {(!string.IsNullOrEmpty(billingAddress.Company) ? $"{billingAddress.Company}<br>" : "")}
                {billingAddress.AddressLine1}<br>
                {(!string.IsNullOrEmpty(billingAddress.AddressLine2) ? $"{billingAddress.AddressLine2}<br>" : "")}
                {billingAddress.City}, {billingAddress.StateProvince} {billingAddress.PostalCode}<br>
                {billingAddress.Country}
                {(!string.IsNullOrEmpty(customerEmail) ? $"<br>{customerEmail}" : "")}
            </div>
        </div>
        " : "")}
        {(shippingAddress != null && showShippingAddress ? $@"
        <div class=""address-block"">
            <div class=""address-title"">Ship To</div>
            <div class=""address-content"">
                <strong>{shippingAddress.FirstName} {shippingAddress.LastName}</strong><br>
                {(!string.IsNullOrEmpty(shippingAddress.Company) ? $"{shippingAddress.Company}<br>" : "")}
                {shippingAddress.AddressLine1}<br>
                {(!string.IsNullOrEmpty(shippingAddress.AddressLine2) ? $"{shippingAddress.AddressLine2}<br>" : "")}
                {shippingAddress.City}, {shippingAddress.StateProvince} {shippingAddress.PostalCode}<br>
                {shippingAddress.Country}
                {(!string.IsNullOrEmpty(shippingAddress.Phone) ? $"<br>Phone: {shippingAddress.Phone}" : "")}
            </div>
        </div>
        " : "")}
    </div>

    {(isGstApplicable ? $@"
    <div style=""margin-bottom: 20px; padding: 12px; background: #fff8e1; border-radius: 4px; border-left: 4px solid #ff9800;"">
        <div style=""display: flex; gap: 40px; font-size: 11px;"">
            {(!string.IsNullOrEmpty(customerGstin) ? $@"<div><strong>Customer GSTIN:</strong> {customerGstin}</div>" : "")}
            {(!string.IsNullOrEmpty(placeOfSupply) ? $@"<div><strong>Place of Supply:</strong> {placeOfSupply}</div>" : "")}
            <div><strong>Transaction Type:</strong> {(isInterState ? "Inter-State (IGST)" : "Intra-State (CGST + SGST)")}</div>
        </div>
    </div>
    " : "")}

    <table>
        <thead>
            <tr>
                <th>Item</th>
                {(showSku ? "<th>SKU</th>" : "")}
                <th class=""text-center"">Qty</th>
                {(!isPackingSlip ? @"<th class=""text-right"">Unit Price</th><th class=""text-right"">Total</th>" : "")}
            </tr>
        </thead>
        <tbody>
            {string.Join("", lineItems.Select(item => $@"
            <tr>
                <td>
                    <strong>{item.ProductName}</strong>
                    {(!string.IsNullOrEmpty(item.VariantName) ? $"<br><small style='color:#666;'>{item.VariantName}</small>" : "")}
                </td>
                {(showSku ? $"<td>{item.Sku}</td>" : "")}
                <td class=""text-center"">{item.Quantity}</td>
                {(!isPackingSlip ? $@"<td class=""text-right"">{FormatCurrency(item.UnitPrice, currencyCode)}</td><td class=""text-right"">{FormatCurrency(item.LineTotal, currencyCode)}</td>" : "")}
            </tr>
            "))}
        </tbody>
    </table>

    {(!isPackingSlip ? $@"
    <table class=""totals"">
        <tr><td class=""label"">Subtotal</td><td class=""text-right"">{FormatCurrency(subtotal, currencyCode)}</td></tr>
        {(discountTotal > 0 ? $@"<tr><td class=""label"">Discount</td><td class=""text-right"" style=""color:green;"">-{FormatCurrency(discountTotal, currencyCode)}</td></tr>" : "")}
        {(shippingTotal > 0 ? $@"<tr><td class=""label"">Shipping</td><td class=""text-right"">{FormatCurrency(shippingTotal, currencyCode)}</td></tr>" : "")}
        {(showTaxBreakdown && isGstApplicable && !isInterState ? $@"
        <tr><td class=""label"">CGST ({cgstRate}%)</td><td class=""text-right"">{FormatCurrency(cgstAmount, currencyCode)}</td></tr>
        <tr><td class=""label"">SGST ({sgstRate}%)</td><td class=""text-right"">{FormatCurrency(sgstAmount, currencyCode)}</td></tr>
        " : "")}
        {(showTaxBreakdown && isGstApplicable && isInterState ? $@"
        <tr><td class=""label"">IGST ({igstRate}%)</td><td class=""text-right"">{FormatCurrency(igstAmount, currencyCode)}</td></tr>
        " : "")}
        {(showTaxBreakdown && !isGstApplicable && taxTotal > 0 ? $@"<tr><td class=""label"">Tax</td><td class=""text-right"">{FormatCurrency(taxTotal, currencyCode)}</td></tr>" : "")}
        <tr class=""grand-total""><td>Total</td><td class=""text-right"">{FormatCurrency(grandTotal, currencyCode)}</td></tr>
        {(paidAmount > 0 ? $@"<tr><td class=""label"">Paid</td><td class=""text-right"">{FormatCurrency(paidAmount, currencyCode)}</td></tr>" : "")}
        {(paidAmount > 0 && grandTotal - paidAmount > 0 ? $@"<tr><td class=""label"" style=""font-weight:bold;"">Balance Due</td><td class=""text-right"" style=""font-weight:bold;color:{primaryColor};"">{FormatCurrency(grandTotal - paidAmount, currencyCode)}</td></tr>" : "")}
    </table>
    " : "")}

    {(!string.IsNullOrEmpty(notes) ? $@"
    <div class=""notes"">
        <div class=""notes-title"">Notes</div>
        {notes}
    </div>
    " : "")}

    {(showPaymentInstructions && !string.IsNullOrEmpty(paymentInstructions) ? $@"
    <div class=""payment-info"">
        <div class=""payment-title"">Payment Instructions</div>
        {paymentInstructions}
    </div>
    " : "")}

    {(!string.IsNullOrEmpty(terms) && !isPackingSlip ? $@"
    <div class=""notes"" style=""margin-top:20px;"">
        <div class=""notes-title"">Terms & Conditions</div>
        {terms}
    </div>
    " : "")}

    {(!string.IsNullOrEmpty(footer) ? $@"<div class=""footer"">{footer}</div>" : "")}

    <script>
        // Auto-print when opened in new window
        if (window.opener) {{
            window.print();
        }}
    </script>
</body>
</html>";

        return html;
    }

    private static string FormatCurrency(decimal amount, string currencyCode)
    {
        return currencyCode.ToUpperInvariant() switch
        {
            "USD" => $"${amount:N2}",
            "EUR" => $"€{amount:N2}",
            "GBP" => $"£{amount:N2}",
            "INR" => $"₹{amount:N2}",
            "JPY" => $"¥{amount:N0}",
            "CAD" => $"C${amount:N2}",
            "AUD" => $"A${amount:N2}",
            _ => $"{currencyCode} {amount:N2}"
        };
    }

    #endregion

    #region Mapping

    private static InvoiceModel MapToModel(Invoice invoice) => new()
    {
        Id = invoice.Id,
        OrderId = invoice.OrderId,
        InvoiceNumber = invoice.InvoiceNumber,
        Status = invoice.Status.ToString(),
        IssuedAt = invoice.IssuedAt,
        DueAt = invoice.DueAt,
        PaidAt = invoice.PaidAt,
        CompanyName = invoice.CompanyName,
        CustomerName = invoice.CustomerName,
        CustomerEmail = invoice.CustomerEmail,
        CurrencyCode = invoice.CurrencyCode,
        Subtotal = invoice.Subtotal,
        DiscountTotal = invoice.DiscountTotal,
        ShippingTotal = invoice.ShippingTotal,
        TaxTotal = invoice.TaxTotal,
        GrandTotal = invoice.GrandTotal,
        PaidAmount = invoice.PaidAmount,
        BalanceDue = invoice.BalanceDue,
        CreatedAt = invoice.CreatedAt
    };

    private static InvoiceTemplateModel MapTemplateToModel(InvoiceTemplate template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Code = template.Code,
        Description = template.Description,
        IsDefault = template.IsDefault,
        IsActive = template.IsActive,
        TemplateType = template.TemplateType.ToString(),
        CompanyName = template.CompanyName,
        CompanyAddress = template.CompanyAddress,
        LogoUrl = template.LogoUrl,
        PrimaryColor = template.PrimaryColor,
        InvoiceTitle = template.InvoiceTitle,
        PackingSlipTitle = template.PackingSlipTitle,
        InvoiceNumberPrefix = template.InvoiceNumberPrefix,
        PaymentTermsDays = template.PaymentTermsDays
    };

    private static InvoiceAddressModel MapAddressToModel(Address address) => new()
    {
        FirstName = address.FirstName,
        LastName = address.LastName,
        Company = address.Company,
        AddressLine1 = address.AddressLine1,
        AddressLine2 = address.AddressLine2,
        City = address.City,
        StateProvince = address.StateProvince,
        PostalCode = address.PostalCode,
        Country = address.Country,
        Phone = address.Phone
    };

    #endregion
}

#region Models

public class InvoiceListResponse
{
    public List<InvoiceModel> Items { get; set; } = [];
    public int Total { get; set; }
}

public class TemplateListResponse
{
    public List<InvoiceTemplateModel> Items { get; set; } = [];
    public int Total { get; set; }
}

public class InvoiceModel
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public required string InvoiceNumber { get; set; }
    public required string Status { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public required string CompanyName { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public required string CurrencyCode { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InvoiceTemplateModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public required string TemplateType { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? LogoUrl { get; set; }
    public required string PrimaryColor { get; set; }
    public required string InvoiceTitle { get; set; }
    public required string PackingSlipTitle { get; set; }
    public required string InvoiceNumberPrefix { get; set; }
    public int PaymentTermsDays { get; set; }
}

public class InvoiceLineItem
{
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? ImageUrl { get; set; }
}

public class InvoiceAddressModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Company { get; set; }
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public required string City { get; set; }
    public string? StateProvince { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
    public string? Phone { get; set; }
}

public class GenerateInvoiceRequest
{
    public Guid? TemplateId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? TaxId { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public string? Footer { get; set; }
    public string? PaymentInstructions { get; set; }

    // GST fields
    public bool IsGstApplicable { get; set; }
    public decimal CgstRate { get; set; }
    public decimal SgstRate { get; set; }
    public decimal IgstRate { get; set; }
    public string? CompanyGstin { get; set; }
    public string? CustomerGstin { get; set; }
    public string? PlaceOfSupply { get; set; }
    public bool IsInterState { get; set; }
}

public class UpdateInvoiceStatusRequest
{
    public required string Status { get; set; }
}

public class CreateTemplateRequest
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public string TemplateType { get; set; } = "Invoice";
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? TaxId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? FontFamily { get; set; }
    public string? CustomCss { get; set; }
    public bool ShowLogo { get; set; } = true;
    public bool ShowShippingAddress { get; set; } = true;
    public bool ShowProductImages { get; set; }
    public bool ShowSku { get; set; } = true;
    public bool ShowTaxBreakdown { get; set; } = true;
    public bool ShowPaymentInstructions { get; set; } = true;
    public string? DefaultNotes { get; set; }
    public string? DefaultTerms { get; set; }
    public string? DefaultFooter { get; set; }
    public string? DefaultPaymentInstructions { get; set; }
    public string? InvoiceTitle { get; set; }
    public string? PackingSlipTitle { get; set; }
    public string? InvoiceNumberPrefix { get; set; }
    public bool IncludeYearInNumber { get; set; } = true;
    public int NumberPadding { get; set; } = 6;
    public string? DateFormat { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
}

public class UpdateTemplateRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? TaxId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? FontFamily { get; set; }
    public string? CustomCss { get; set; }
    public bool? ShowLogo { get; set; }
    public bool? ShowShippingAddress { get; set; }
    public bool? ShowProductImages { get; set; }
    public bool? ShowSku { get; set; }
    public bool? ShowTaxBreakdown { get; set; }
    public bool? ShowPaymentInstructions { get; set; }
    public string? DefaultNotes { get; set; }
    public string? DefaultTerms { get; set; }
    public string? DefaultFooter { get; set; }
    public string? DefaultPaymentInstructions { get; set; }
    public string? InvoiceTitle { get; set; }
    public string? PackingSlipTitle { get; set; }
    public string? InvoiceNumberPrefix { get; set; }
    public bool? IncludeYearInNumber { get; set; }
    public int? NumberPadding { get; set; }
    public string? DateFormat { get; set; }
    public int? PaymentTermsDays { get; set; }
}

#endregion
