import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Purchase Order Receiving
 * Receive items from the purchase order.
 */
export class PurchaseOrderReceiving extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .receiving-grid {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: var(--uui-size-layout-1);
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

    .number-cell {
      font-variant-numeric: tabular-nums;
      text-align: right;
    }

    .input-cell {
      width: 100px;
    }

    .input-cell uui-input {
      width: 80px;
    }

    .status-row-pending {
      background: var(--uui-color-surface);
    }

    .status-row-partial {
      background: var(--uui-color-warning-emphasis);
    }

    .status-row-complete {
      background: var(--uui-color-positive-emphasis);
    }

    .summary-box {
      position: sticky;
      top: var(--uui-size-layout-1);
    }

    .summary-item {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-3) 0;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .summary-item:last-child {
      border-bottom: none;
    }

    .summary-value {
      font-weight: 500;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-4);
      color: var(--uui-color-text-alt);
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-4);
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _items: { type: Array, state: true },
    _receivingQty: { type: Object, state: true }
  };

  constructor() {
    super();
    this._order = {};
    this._items = [];
    this._receivingQty = {};
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-purchase-order-workspace');
      if (workspace) {
        this._order = workspace.getOrder();
        this._items = this._order.items || [];
        // Initialize receiving quantities
        this._items.forEach(item => {
          const remaining = item.quantityOrdered - (item.quantityReceived || 0);
          this._receivingQty[item.id] = { receive: 0, reject: 0, remaining };
        });
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateReceiving(itemId, field, value) {
    this._receivingQty = {
      ...this._receivingQty,
      [itemId]: {
        ...this._receivingQty[itemId],
        [field]: parseInt(value) || 0
      }
    };
  }

  _getTotalReceiving() {
    return Object.values(this._receivingQty).reduce((sum, qty) => sum + (qty.receive || 0), 0);
  }

  _getTotalRejecting() {
    return Object.values(this._receivingQty).reduce((sum, qty) => sum + (qty.reject || 0), 0);
  }

  _canReceive() {
    return this._order.status === 'Sent' || this._order.status === 'PartiallyReceived';
  }

  async _processReceiving() {
    const itemsToReceive = Object.entries(this._receivingQty)
      .filter(([id, qty]) => qty.receive > 0 || qty.reject > 0)
      .map(([itemId, qty]) => ({
        itemId,
        quantityReceived: qty.receive,
        quantityRejected: qty.reject
      }));

    if (itemsToReceive.length === 0) {
      alert('Please enter quantities to receive');
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/purchase-order/${this._order.id}/receive`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ items: itemsToReceive })
      });

      if (response.ok) {
        // Reload the order
        window.location.reload();
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to process receiving');
      }
    } catch (error) {
      console.error('Error processing receiving:', error);
      alert('Failed to process receiving');
    }
  }

  render() {
    const canReceive = this._canReceive();

    return html`
      <div class="receiving-grid">
        <uui-box headline="Receive Items">
          ${!canReceive ? html`
            <div class="help-text">
              <uui-icon name="icon-info"></uui-icon>
              Items can only be received when the order status is "Sent to Supplier" or "Partially Received".
            </div>
          ` : html`
            <div class="help-text">
              Enter the quantities received and any rejected items.
            </div>
          `}

          ${this._items.length === 0 ? html`
            <div class="empty-state">
              <p>No items to receive</p>
            </div>
          ` : html`
            <table class="items-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th class="number-cell">Ordered</th>
                  <th class="number-cell">Received</th>
                  <th class="number-cell">Remaining</th>
                  <th class="input-cell">Receive</th>
                  <th class="input-cell">Reject</th>
                </tr>
              </thead>
              <tbody>
                ${this._items.map(item => {
                  const qty = this._receivingQty[item.id] || { receive: 0, reject: 0, remaining: 0 };
                  return html`
                    <tr>
                      <td>
                        ${item.productName || 'Unknown Product'}
                        <br>
                        <small style="color: var(--uui-color-text-alt)">${item.sku}</small>
                      </td>
                      <td class="number-cell">${item.quantityOrdered}</td>
                      <td class="number-cell">${item.quantityReceived || 0}</td>
                      <td class="number-cell">${qty.remaining}</td>
                      <td class="input-cell">
                        <uui-input
                          type="number"
                          min="0"
                          max="${qty.remaining}"
                          .value=${qty.receive}
                          ?disabled=${!canReceive}
                          @input=${(e) => this._updateReceiving(item.id, 'receive', e.target.value)}
                        ></uui-input>
                      </td>
                      <td class="input-cell">
                        <uui-input
                          type="number"
                          min="0"
                          max="${qty.remaining}"
                          .value=${qty.reject}
                          ?disabled=${!canReceive}
                          @input=${(e) => this._updateReceiving(item.id, 'reject', e.target.value)}
                        ></uui-input>
                      </td>
                    </tr>
                  `;
                })}
              </tbody>
            </table>
          `}
        </uui-box>

        <div class="summary-box">
          <uui-box headline="Receiving Summary">
            <div class="summary-item">
              <span>Items to Receive</span>
              <span class="summary-value">${this._getTotalReceiving()}</span>
            </div>
            <div class="summary-item">
              <span>Items to Reject</span>
              <span class="summary-value">${this._getTotalRejecting()}</span>
            </div>
            <div class="summary-item">
              <span>Order Status</span>
              <span class="summary-value">${this._order.status}</span>
            </div>

            <uui-button
              look="primary"
              style="width: 100%; margin-top: var(--uui-size-space-4);"
              ?disabled=${!canReceive || this._getTotalReceiving() === 0}
              @click=${this._processReceiving}
            >
              Process Receiving
            </uui-button>
          </uui-box>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-purchase-order-receiving', PurchaseOrderReceiving);

export default PurchaseOrderReceiving;
