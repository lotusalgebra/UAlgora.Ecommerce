import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Purchase Order Items
 * Manage line items on the purchase order.
 */
export class PurchaseOrderItems extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .items-table {
      width: 100%;
      border-collapse: collapse;
    }

    .items-table th,
    .items-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .items-table th {
      background: var(--uui-color-surface-alt);
      font-weight: 500;
    }

    .items-table tbody tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .items-table tfoot {
      font-weight: 500;
    }

    .items-table tfoot td {
      border-top: 2px solid var(--uui-color-border);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-4);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .number-cell {
      font-variant-numeric: tabular-nums;
      text-align: right;
    }

    .quantity-input {
      width: 80px;
    }

    .price-input {
      width: 100px;
    }

    .actions-cell {
      width: 100px;
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-pending {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .status-partial {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .status-complete {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _items: { type: Array, state: true }
  };

  constructor() {
    super();
    this._order = {};
    this._items = [];
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-purchase-order-workspace');
      if (workspace) {
        this._order = workspace.getOrder();
        this._items = this._order.items || [];
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this._order.currencyCode || 'USD'
    }).format(amount || 0);
  }

  _getItemStatus(item) {
    if (item.quantityReceived >= item.quantityOrdered) return 'complete';
    if (item.quantityReceived > 0) return 'partial';
    return 'pending';
  }

  _getStatusLabel(status) {
    const labels = {
      'pending': 'Pending',
      'partial': 'Partial',
      'complete': 'Complete'
    };
    return labels[status] || status;
  }

  _calculateSubtotal() {
    return this._items.reduce((sum, item) => sum + (item.lineTotal || 0), 0);
  }

  render() {
    return html`
      <uui-box>
        <div class="toolbar">
          <h4 style="margin: 0;">Order Items</h4>
          <uui-button look="primary">
            <uui-icon name="icon-add"></uui-icon>
            Add Item
          </uui-button>
        </div>

        ${this._items.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-box"></uui-icon>
            <p>No items added to this purchase order</p>
            <uui-button look="primary">Add First Item</uui-button>
          </div>
        ` : html`
          <table class="items-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th>Supplier SKU</th>
                <th class="number-cell">Qty Ordered</th>
                <th class="number-cell">Qty Received</th>
                <th class="number-cell">Unit Cost</th>
                <th class="number-cell">Line Total</th>
                <th>Status</th>
                <th class="actions-cell"></th>
              </tr>
            </thead>
            <tbody>
              ${this._items.map((item, index) => {
                const status = this._getItemStatus(item);
                return html`
                  <tr>
                    <td>${item.productName || 'Unknown Product'}</td>
                    <td>${item.sku || '-'}</td>
                    <td>${item.supplierSku || '-'}</td>
                    <td class="number-cell">${item.quantityOrdered}</td>
                    <td class="number-cell">${item.quantityReceived || 0}</td>
                    <td class="number-cell">${this._formatCurrency(item.unitCost)}</td>
                    <td class="number-cell">${this._formatCurrency(item.lineTotal)}</td>
                    <td>
                      <span class="status-badge status-${status}">
                        ${this._getStatusLabel(status)}
                      </span>
                    </td>
                    <td class="actions-cell">
                      <uui-button compact look="secondary">Edit</uui-button>
                      <uui-button compact look="secondary" color="danger">
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </td>
                  </tr>
                `;
              })}
            </tbody>
            <tfoot>
              <tr>
                <td colspan="6" style="text-align: right;">Subtotal</td>
                <td class="number-cell">${this._formatCurrency(this._calculateSubtotal())}</td>
                <td colspan="2"></td>
              </tr>
            </tfoot>
          </table>
        `}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-purchase-order-items', PurchaseOrderItems);

export default PurchaseOrderItems;
