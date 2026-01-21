import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Workspace
 * Main container for stock transfer editing.
 */
export class StockTransferWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
    }

    .workspace-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-layout-1);
      border-bottom: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
    }

    .header-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
    }

    .header-info uui-icon {
      font-size: 32px;
      color: var(--uui-color-interactive);
    }

    .header-title h1 {
      margin: 0;
      font-size: var(--uui-type-h4-size);
    }

    .header-meta {
      display: flex;
      gap: var(--uui-size-space-3);
      margin-top: var(--uui-size-space-1);
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-draft { background: var(--uui-color-surface-alt); color: var(--uui-color-text-alt); }
    .status-pending { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }
    .status-approved { background: var(--uui-color-default-emphasis); color: var(--uui-color-default); }
    .status-intransit { background: var(--uui-color-default-emphasis); color: var(--uui-color-default); }
    .status-partiallyreceived { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }
    .status-completed { background: var(--uui-color-positive-emphasis); color: var(--uui-color-positive); }
    .status-cancelled { background: var(--uui-color-danger-emphasis); color: var(--uui-color-danger); }

    .badge-source {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text);
    }

    .badge-destination {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }

    .transfer-arrow {
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _transfer: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._transfer = {
      referenceNumber: '',
      sourceWarehouseId: null,
      sourceWarehouseName: '',
      destinationWarehouseId: null,
      destinationWarehouseName: '',
      status: 'Draft',
      requestedDate: new Date().toISOString().split('T')[0],
      expectedArrivalDate: null,
      shippedDate: null,
      receivedDate: null,
      notes: '',
      trackingNumber: '',
      carrier: '',
      items: []
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTransfer();
  }

  async _loadTransfer() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/stock-transfer\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const transferId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/stock-transfer/${transferId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._transfer = await response.json();
        }
      } catch (error) {
        console.error('Error loading stock transfer:', error);
      }
    }
  }

  getTransfer() {
    return this._transfer;
  }

  setTransfer(transfer) {
    this._transfer = { ...transfer };
    this.requestUpdate();
  }

  isNewTransfer() {
    return this._isNew;
  }

  _getStatusLabel(status) {
    const labels = {
      'Draft': 'Draft',
      'Pending': 'Pending Approval',
      'Approved': 'Approved',
      'InTransit': 'In Transit',
      'PartiallyReceived': 'Partially Received',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled'
    };
    return labels[status] || status;
  }

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-split"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Stock Transfer' : `Transfer #${this._transfer.referenceNumber}`}</h1>
            <div class="header-meta">
              <span class="status-badge status-${this._transfer.status?.toLowerCase()}">
                ${this._getStatusLabel(this._transfer.status)}
              </span>
              ${this._transfer.sourceWarehouseName ? html`
                <span class="status-badge badge-source">${this._transfer.sourceWarehouseName}</span>
                <span class="transfer-arrow">→</span>
                <span class="status-badge badge-destination">${this._transfer.destinationWarehouseName}</span>
              ` : ''}
            </div>
          </div>
        </div>
        <slot name="action-menu"></slot>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-workspace', StockTransferWorkspace);

export default StockTransferWorkspace;
