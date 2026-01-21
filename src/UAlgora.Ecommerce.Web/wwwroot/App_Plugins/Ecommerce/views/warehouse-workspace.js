import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Workspace
 * Main container for warehouse editing.
 */
export class WarehouseWorkspace extends UmbElementMixin(LitElement) {
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

    .status-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .status-inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .badge-default {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .badge-type {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }
  `;

  static properties = {
    _warehouse: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._warehouse = {
      name: '',
      code: '',
      type: 'Warehouse',
      description: '',
      isActive: true,
      isDefault: false,
      canFulfillOrders: true,
      acceptsReturns: true,
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      postalCode: '',
      country: '',
      contactName: '',
      contactEmail: '',
      contactPhone: '',
      priority: 0,
      shippingCountries: [],
      operatingHours: {}
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadWarehouse();
  }

  async _loadWarehouse() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/warehouse\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const warehouseId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouseId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._warehouse = await response.json();
        }
      } catch (error) {
        console.error('Error loading warehouse:', error);
      }
    }
  }

  getWarehouse() {
    return this._warehouse;
  }

  setWarehouse(warehouse) {
    this._warehouse = { ...warehouse };
    this.requestUpdate();
  }

  isNewWarehouse() {
    return this._isNew;
  }

  _getTypeLabel(type) {
    const types = {
      'Warehouse': 'Warehouse',
      'Store': 'Retail Store',
      'DistributionCenter': 'Distribution Center',
      'DropShip': 'Drop Ship',
      'Virtual': 'Virtual'
    };
    return types[type] || type;
  }

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-box-open"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Warehouse' : this._warehouse.name || 'Warehouse'}</h1>
            <div class="header-meta">
              <span class="status-badge ${this._warehouse.isActive ? 'status-active' : 'status-inactive'}">
                ${this._warehouse.isActive ? 'Active' : 'Inactive'}
              </span>
              ${this._warehouse.isDefault ? html`
                <span class="status-badge badge-default">Default</span>
              ` : ''}
              <span class="status-badge badge-type">${this._getTypeLabel(this._warehouse.type)}</span>
            </div>
          </div>
        </div>
        <slot name="action-menu"></slot>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-warehouse-workspace', WarehouseWorkspace);

export default WarehouseWorkspace;
