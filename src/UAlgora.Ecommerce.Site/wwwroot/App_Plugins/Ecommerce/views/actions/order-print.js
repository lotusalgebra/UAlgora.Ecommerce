import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Print Action
 * Quick action to print/export order invoice.
 */
export class OrderPrintAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
  }

  _formatCurrency(amount, currencyCode) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode || 'USD'
    }).format(amount || 0);
  }

  _formatAddress(address) {
    if (!address) return 'N/A';
    const parts = [
      `${address.firstName} ${address.lastName}`.trim(),
      address.company,
      address.addressLine1,
      address.addressLine2,
      `${address.city}, ${address.stateProvince} ${address.postalCode}`.trim(),
      address.country
    ].filter(Boolean);
    return parts.join('<br>');
  }

  async _handlePrint() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/print`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const printData = await response.json();
        this._openPrintWindow(printData);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to get print data');
      }
    } catch (error) {
      console.error('Error getting print data:', error);
      alert('Failed to get print data');
    } finally {
      this._processing = false;
    }
  }

  _openPrintWindow(data) {
    const linesHtml = data.lines.map(line => `
      <tr>
        <td>${line.productName}${line.variantName ? `<br><small>${line.variantName}</small>` : ''}</td>
        <td>${line.sku}</td>
        <td style="text-align: center;">${line.quantity}</td>
        <td style="text-align: right;">${this._formatCurrency(line.unitPrice, data.currencyCode)}</td>
        <td style="text-align: right;">${this._formatCurrency(line.finalLineTotal, data.currencyCode)}</td>
      </tr>
    `).join('');

    const printContent = `
      <!DOCTYPE html>
      <html>
      <head>
        <title>Order ${data.orderNumber}</title>
        <style>
          body { font-family: Arial, sans-serif; font-size: 12px; line-height: 1.4; padding: 20px; }
          h1 { font-size: 24px; margin-bottom: 5px; }
          .header { display: flex; justify-content: space-between; margin-bottom: 30px; }
          .order-info { text-align: right; }
          .addresses { display: flex; gap: 40px; margin-bottom: 30px; }
          .address-box { flex: 1; }
          .address-box h3 { margin-bottom: 10px; font-size: 14px; border-bottom: 1px solid #ccc; padding-bottom: 5px; }
          table { width: 100%; border-collapse: collapse; margin-bottom: 30px; }
          th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
          th { background: #f5f5f5; font-weight: bold; }
          .totals { width: 300px; margin-left: auto; }
          .totals td { padding: 5px 10px; }
          .totals .total-row { font-weight: bold; font-size: 16px; border-top: 2px solid #333; }
          .notes { margin-top: 30px; padding: 15px; background: #f9f9f9; border-radius: 5px; }
          .notes h3 { margin-top: 0; }
          .footer { margin-top: 40px; text-align: center; font-size: 10px; color: #666; }
          @media print {
            body { padding: 0; }
            .no-print { display: none; }
          }
        </style>
      </head>
      <body>
        <div class="header">
          <div>
            <h1>INVOICE</h1>
            <p>Order #${data.orderNumber}</p>
          </div>
          <div class="order-info">
            <p><strong>Date:</strong> ${new Date(data.createdAt).toLocaleDateString()}</p>
            <p><strong>Status:</strong> ${data.status}</p>
          </div>
        </div>

        <div class="addresses">
          <div class="address-box">
            <h3>Bill To</h3>
            ${this._formatAddress(data.billingAddress)}
          </div>
          <div class="address-box">
            <h3>Ship To</h3>
            ${this._formatAddress(data.shippingAddress)}
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>Item</th>
              <th>SKU</th>
              <th style="text-align: center;">Qty</th>
              <th style="text-align: right;">Unit Price</th>
              <th style="text-align: right;">Total</th>
            </tr>
          </thead>
          <tbody>
            ${linesHtml}
          </tbody>
        </table>

        <table class="totals">
          <tr>
            <td>Subtotal:</td>
            <td style="text-align: right;">${this._formatCurrency(data.subtotal, data.currencyCode)}</td>
          </tr>
          ${data.discountTotal > 0 ? `
          <tr>
            <td>Discount:</td>
            <td style="text-align: right;">-${this._formatCurrency(data.discountTotal, data.currencyCode)}</td>
          </tr>
          ` : ''}
          <tr>
            <td>Shipping:</td>
            <td style="text-align: right;">${this._formatCurrency(data.shippingTotal, data.currencyCode)}</td>
          </tr>
          <tr>
            <td>Tax:</td>
            <td style="text-align: right;">${this._formatCurrency(data.taxTotal, data.currencyCode)}</td>
          </tr>
          <tr class="total-row">
            <td>Grand Total:</td>
            <td style="text-align: right;">${this._formatCurrency(data.grandTotal, data.currencyCode)}</td>
          </tr>
        </table>

        ${data.customerNote ? `
        <div class="notes">
          <h3>Customer Notes</h3>
          <p>${data.customerNote}</p>
        </div>
        ` : ''}

        <div class="footer">
          <p>Thank you for your order!</p>
          <p>Printed on ${new Date(data.printedAt).toLocaleString()}</p>
        </div>

        <script>
          window.onload = function() { window.print(); }
        </script>
      </body>
      </html>
    `;

    const printWindow = window.open('', '_blank', 'width=800,height=600');
    printWindow.document.write(printContent);
    printWindow.document.close();
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handlePrint}
      >
        <uui-icon name="icon-print"></uui-icon>
        ${this._processing ? 'Loading...' : 'Print Invoice'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-print-action', OrderPrintAction);

export default OrderPrintAction;
