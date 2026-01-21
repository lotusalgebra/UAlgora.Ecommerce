import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Items
 * Manage line items on the stock adjustment.
 */
export class StockAdjustmentItems extends UmbElementMixin(LitElement) {
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
      width: 100px;
    }

    .actions-cell {
      width: 100px;
    }

    .positive {
      color: var(--uui-color-positive);
    }

    .negative {
      color: var(--uui-color-danger);
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-4);
    }
  `;

  static properties = {
    _adjustment: { type: Object, state: true },
    _items: { type: Array, state: true }
  };

  constructor() {
    super();
    this._adjustment = {};
    this._items = [];
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-stock-adjustment-workspace');
      if (workspace) {
        this._adjustment = workspace.getAdjustment();
        this._items = this._adjustment.items || [];
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _formatCurrency(amount) {
    if (amount == null) return '-';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  _formatQuantity(qty) {
    if (qty > 0) return `+${qty}`;
    return qty.toString();
  }

  _getTotalQuantity() {
    return this._items.reduce((sum, item) => sum + (item.quantityAdjustment || 0), 0);
  }

  _getTotalValue() {
    return this._items.reduce((sum, item) => {
      const cost = item.unitCost || 0;
      const qty = item.quantityAdjustment || 0;
      return sum + (cost * qty);
    }, 0);
  }

  _isEditable() {
    return this._adjustment.status === 'Draft' || this._adjustment.status === 'Pending';
  }

  render() {
    const editable = this._isEditable();

    return html`
      <uui-box>
        <div class="toolbar">
          <div>
            <h4 style="margin: 0;">Adjustment Items</h4>
            ${!editable ? html`
              <p class="help-text" style="margin-top: var(--uui-size-space-2);">
                Items cannot be modified after the adjustment has been approved.
              </p>
            ` : ''}
          </div>
          ${editable ? html`
            <uui-button look="primary">
              <uui-icon name="icon-add"></uui-icon>
              Add Item
            </uui-button>
          ` : ''}
        </div>

        ${this._items.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-box"></uui-icon>
            <p>No items added to this adjustment</p>
            ${editable ? html`
              <uui-button look="primary">Add First Item</uui-button>
            ` : ''}
          </div>
        ` : html`
          <table class="items-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th>Bin Location</th>
                <th class="number-cell">Current Qty</th>
                <th class="number-cell">Adjustment</th>
                <th class="number-cell">New Qty</th>
                <th class="number-cell">Unit Cost</th>
                <th class="number-cell">Value</th>
                ${editable ? html`<th class="actions-cell"></th>` : ''}
              </tr>
            </thead>
            <tbody>
              ${this._items.map((item, index) => {
                const adjustment = item.quantityAdjustment || 0;
                const newQty = (item.quantityBefore || 0) + adjustment;
                const value = (item.unitCost || 0) * adjustment;
                return html`
                  <tr>
                    <td>${item.productName || 'Unknown Product'}</td>
                    <td>${item.sku || '-'}</td>
                    <td>${item.binLocation || '-'}</td>
                    <td class="number-cell">${item.quantityBefore || 0}</td>
                    <td class="number-cell ${adjustment >= 0 ? 'positive' : 'negative'}">
                      ${this._formatQuantity(adjustment)}
                    </td>
                    <td class="number-cell">${newQty}</td>
                    <td class="number-cell">${this._formatCurrency(item.unitCost)}</td>
                    <td class="number-cell ${value >= 0 ? 'positive' : 'negative'}">
                      ${this._formatCurrency(value)}
                    </td>
                    ${editable ? html`
                      <td class="actions-cell">
                        <uui-button compact look="secondary">Edit</uui-button>
                        <uui-button compact look="secondary" color="danger">
                          <uui-icon name="icon-delete"></uui-icon>
                        </uui-button>
                      </td>
                    ` : ''}
                  </tr>
                `;
              })}
            </tbody>
            <tfoot>
              <tr>
                <td colspan="4" style="text-align: right;">Total</td>
                <td class="number-cell ${this._getTotalQuantity() >= 0 ? 'positive' : 'negative'}">
                  ${this._formatQuantity(this._getTotalQuantity())}
                </td>
                <td colspan="2"></td>
                <td class="number-cell ${this._getTotalValue() >= 0 ? 'positive' : 'negative'}">
                  ${this._formatCurrency(this._getTotalValue())}
                </td>
                ${editable ? html`<td></td>` : ''}
              </tr>
            </tfoot>
          </table>
        `}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-stock-adjustment-items', StockAdjustmentItems);

export default StockAdjustmentItems;
