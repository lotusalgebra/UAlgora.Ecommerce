import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Items View
 * Displays all line items in the order.
 */
export class OrderItems extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .items-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .items-table th,
    .items-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .items-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .items-table tr:last-child td {
      border-bottom: none;
    }

    .items-table tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .product-cell {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
    }

    .product-image {
      width: 60px;
      height: 60px;
      object-fit: cover;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .product-info {
      flex: 1;
    }

    .product-name {
      font-weight: 500;
      color: var(--uui-color-text);
    }

    .product-variant {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .product-sku {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      font-family: monospace;
    }

    .price-cell {
      text-align: right;
    }

    .original-price {
      text-decoration: line-through;
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .discount-amount {
      color: var(--uui-color-positive);
      font-size: var(--uui-type-small-size);
    }

    .quantity-cell {
      text-align: center;
    }

    .total-cell {
      text-align: right;
      font-weight: 500;
    }

    .summary-row {
      background: var(--uui-color-surface-alt);
    }

    .summary-row td {
      font-weight: bold;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _order: { type: Object, state: true }
  };

  constructor() {
    super();
    this._order = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
    }
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode
    }).format(amount || 0);
  }

  _handleProductClick(productId) {
    if (productId) {
      window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/product/edit/${productId}`);
    }
  }

  render() {
    if (!this._order) {
      return html`<uui-loader></uui-loader>`;
    }

    const lines = this._order.lines || [];
    const currency = this._order.currencyCode || 'USD';

    if (lines.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-box" style="font-size: 48px;"></uui-icon>
          <p>No items in this order</p>
        </div>
      `;
    }

    return html`
      <uui-box>
        <div slot="headline">Order Items (${lines.length})</div>

        <table class="items-table">
          <thead>
            <tr>
              <th>Product</th>
              <th class="price-cell">Unit Price</th>
              <th class="quantity-cell">Qty</th>
              <th class="price-cell">Discount</th>
              <th class="price-cell">Tax</th>
              <th class="total-cell">Total</th>
            </tr>
          </thead>
          <tbody>
            ${lines.map(line => html`
              <tr @click=${() => this._handleProductClick(line.productId)} style="cursor: pointer;">
                <td>
                  <div class="product-cell">
                    ${line.imageUrl
                      ? html`<img class="product-image" src="${line.imageUrl}" alt="${line.productName}" />`
                      : html`<div class="product-image"><uui-icon name="icon-box"></uui-icon></div>`
                    }
                    <div class="product-info">
                      <div class="product-name">${line.productName}</div>
                      ${line.variantName ? html`
                        <div class="product-variant">${line.variantName}</div>
                      ` : ''}
                      <div class="product-sku">SKU: ${line.sku}</div>
                    </div>
                  </div>
                </td>
                <td class="price-cell">
                  ${this._formatCurrency(line.unitPrice, currency)}
                </td>
                <td class="quantity-cell">
                  ${line.quantity}
                </td>
                <td class="price-cell">
                  ${line.discountAmount > 0
                    ? html`<span class="discount-amount">-${this._formatCurrency(line.discountAmount, currency)}</span>`
                    : '-'
                  }
                </td>
                <td class="price-cell">
                  ${this._formatCurrency(line.taxAmount, currency)}
                </td>
                <td class="total-cell">
                  ${this._formatCurrency(line.finalLineTotal, currency)}
                </td>
              </tr>
            `)}
            <tr class="summary-row">
              <td colspan="5" style="text-align: right;">Subtotal</td>
              <td class="total-cell">${this._formatCurrency(this._order.subtotal, currency)}</td>
            </tr>
            ${this._order.discountTotal > 0 ? html`
              <tr class="summary-row">
                <td colspan="5" style="text-align: right;">Discount</td>
                <td class="total-cell" style="color: var(--uui-color-positive);">
                  -${this._formatCurrency(this._order.discountTotal, currency)}
                </td>
              </tr>
            ` : ''}
            <tr class="summary-row">
              <td colspan="5" style="text-align: right;">Shipping</td>
              <td class="total-cell">${this._formatCurrency(this._order.shippingTotal, currency)}</td>
            </tr>
            <tr class="summary-row">
              <td colspan="5" style="text-align: right;">Tax</td>
              <td class="total-cell">${this._formatCurrency(this._order.taxTotal, currency)}</td>
            </tr>
            <tr class="summary-row" style="font-size: var(--uui-type-h4-size);">
              <td colspan="5" style="text-align: right;">Grand Total</td>
              <td class="total-cell">${this._formatCurrency(this._order.grandTotal, currency)}</td>
            </tr>
          </tbody>
        </table>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-order-items', OrderItems);

export default OrderItems;
