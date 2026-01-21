import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Workspace
 * Main container for stock adjustment editing.
 */
export class StockAdjustmentWorkspace extends UmbElementMixin(LitElement) {
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
    .status-completed { background: var(--uui-color-positive-emphasis); color: var(--uui-color-positive); }
    .status-cancelled { background: var(--uui-color-danger-emphasis); color: var(--uui-color-danger); }
    .status-rejected { background: var(--uui-color-danger-emphasis); color: var(--uui-color-danger); }

    .badge-type {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }

    .badge-warehouse {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text);
    }
  `;

  static properties = {
    _adjustment: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._adjustment = {
      referenceNumber: '',
      warehouseId: null,
      warehouseName: '',
      type: 'InventoryCount',
      status: 'Draft',
      adjustmentDate: new Date().toISOString().split('T')[0],
      notes: '',
      externalReference: '',
      createdBy: '',
      items: []
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadAdjustment();
  }

  async _loadAdjustment() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/stock-adjustment\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const adjustmentId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/stock-adjustment/${adjustmentId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._adjustment = await response.json();
        }
      } catch (error) {
        console.error('Error loading stock adjustment:', error);
      }
    }
  }

  getAdjustment() {
    return this._adjustment;
  }

  setAdjustment(adjustment) {
    this._adjustment = { ...adjustment };
    this.requestUpdate();
  }

  isNewAdjustment() {
    return this._isNew;
  }

  _getTypeLabel(type) {
    const types = {
      'InventoryCount': 'Inventory Count',
      'Damage': 'Damage',
      'Theft': 'Theft',
      'Expired': 'Expired',
      'CustomerReturn': 'Customer Return',
      'SupplierReturn': 'Supplier Return',
      'WriteOff': 'Write Off',
      'Found': 'Found',
      'Correction': 'Correction',
      'Other': 'Other'
    };
    return types[type] || type;
  }

  _getStatusLabel(status) {
    const labels = {
      'Draft': 'Draft',
      'Pending': 'Pending Approval',
      'Approved': 'Approved',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled',
      'Rejected': 'Rejected'
    };
    return labels[status] || status;
  }

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-axis-rotation"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Stock Adjustment' : `Adjustment #${this._adjustment.referenceNumber}`}</h1>
            <div class="header-meta">
              <span class="status-badge status-${this._adjustment.status?.toLowerCase()}">
                ${this._getStatusLabel(this._adjustment.status)}
              </span>
              <span class="status-badge badge-type">${this._getTypeLabel(this._adjustment.type)}</span>
              ${this._adjustment.warehouseName ? html`
                <span class="status-badge badge-warehouse">${this._adjustment.warehouseName}</span>
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

customElements.define('ecommerce-stock-adjustment-workspace', StockAdjustmentWorkspace);

export default StockAdjustmentWorkspace;
