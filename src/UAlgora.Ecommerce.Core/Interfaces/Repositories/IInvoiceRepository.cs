using UAlgora.Ecommerce.Core.Models.Domain;

namespace UAlgora.Ecommerce.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for invoice operations.
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice>
{
    /// <summary>
    /// Gets an invoice by invoice number.
    /// </summary>
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>
    /// Gets invoices for an order.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets the latest invoice for an order.
    /// </summary>
    Task<Invoice?> GetLatestByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets invoices by status.
    /// </summary>
    Task<IReadOnlyList<Invoice>> GetByStatusAsync(InvoiceStatus status, CancellationToken ct = default);

    /// <summary>
    /// Generates a new unique invoice number.
    /// </summary>
    Task<string> GenerateInvoiceNumberAsync(string prefix = "INV-", bool includeYear = true, int padding = 6, CancellationToken ct = default);

    /// <summary>
    /// Gets all invoice templates.
    /// </summary>
    Task<IReadOnlyList<InvoiceTemplate>> GetTemplatesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the default invoice template.
    /// </summary>
    Task<InvoiceTemplate?> GetDefaultTemplateAsync(InvoiceTemplateType type = InvoiceTemplateType.Invoice, CancellationToken ct = default);

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    Task<InvoiceTemplate?> GetTemplateByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a template by code.
    /// </summary>
    Task<InvoiceTemplate?> GetTemplateByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Adds a new template.
    /// </summary>
    Task<InvoiceTemplate> AddTemplateAsync(InvoiceTemplate template, CancellationToken ct = default);

    /// <summary>
    /// Updates a template.
    /// </summary>
    Task<InvoiceTemplate> UpdateTemplateAsync(InvoiceTemplate template, CancellationToken ct = default);

    /// <summary>
    /// Deletes a template.
    /// </summary>
    Task DeleteTemplateAsync(Guid id, CancellationToken ct = default);
}
