import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Items
 * Manage line items on the stock transfer.
 */
export class StockTransferItems extends UmbElementMixin(LitElement) {
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

    .status-shipped {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }

    .status-partial {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .status-complete {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-4);
    }
  `;

  static properties = {
    _transfer: { type: Object, state: true },
    _items: { type: Array, state: true }
  };

  constructor() {
    super();
    this._transfer = {};
    this._items = [];
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-stock-transfer-workspace');
      if (workspace) {
        this._transfer = workspace.getTransfer();
        this._items = this._transfer.items || [];
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _getItemStatus(item) {
    if (item.quantityReceived >= item.quantityRequested) return 'complete';
    if (item.quantityReceived > 0) return 'partial';
    if (item.quantityShipped > 0) return 'shipped';
    return 'pending';
  }

  _getStatusLabel(status) {
    const labels = {
      'pending': 'Pending',
      'shipped': 'Shipped',
      'partial': 'Partial',
      'complete': 'Complete'
    };
    return labels[status] || status;
  }

  _getTotalRequested() {
    return this._items.reduce((sum, item) => sum + (item.quantityRequested || 0), 0);
  }

  _getTotalShipped() {
    return this._items.reduce((sum, item) => sum + (item.quantityShipped || 0), 0);
  }

  _getTotalReceived() {
    return this._items.reduce((sum, item) => sum + (item.quantityReceived || 0), 0);
  }

  _isEditable() {
    return this._transfer.status === 'Draft' || this._transfer.status === 'Pending';
  }

  render() {
    const editable = this._isEditable();

    return html`
      <uui-box>
        <div class="toolbar">
          <div>
            <h4 style="margin: 0;">Transfer Items</h4>
            ${!editable ? html`
              <p class="help-text" style="margin-top: var(--uui-size-space-2);">
                Items cannot be modified after the transfer has been approved.
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
            <p>No items added to this transfer</p>
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
                <th>Source Bin</th>
                <th>Dest Bin</th>
                <th class="number-cell">Requested</th>
                <th class="number-cell">Shipped</th>
                <th class="number-cell">Received</th>
                <th>Status</th>
                ${editable ? html`<th class="actions-cell"></th>` : ''}
              </tr>
            </thead>
            <tbody>
              ${this._items.map((item, index) => {
                const status = this._getItemStatus(item);
                return html`
                  <tr>
                    <td>${item.productName || 'Unknown Product'}</td>
                    <td>${item.sku || '-'}</td>
                    <td>${item.sourceBinLocation || '-'}</td>
                    <td>${item.destinationBinLocation || '-'}</td>
                    <td class="number-cell">${item.quantityRequested || 0}</td>
                    <td class="number-cell">${item.quantityShipped || 0}</td>
                    <td class="number-cell">${item.quantityReceived || 0}</td>
                    <td>
                      <span class="status-badge status-${status}">
                        ${this._getStatusLabel(status)}
                      </span>
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
                <td colspan="4" style="text-align: right;">Totals</td>
                <td class="number-cell">${this._getTotalRequested()}</td>
                <td class="number-cell">${this._getTotalShipped()}</td>
                <td class="number-cell">${this._getTotalReceived()}</td>
                <td></td>
                ${editable ? html`<td></td>` : ''}
              </tr>
            </tfoot>
          </table>
        `}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-items', StockTransferItems);

export default StockTransferItems;
