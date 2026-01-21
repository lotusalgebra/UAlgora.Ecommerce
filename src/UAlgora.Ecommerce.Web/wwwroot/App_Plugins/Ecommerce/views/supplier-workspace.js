import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Workspace
 * Main container for supplier editing.
 */
export class SupplierWorkspace extends UmbElementMixin(LitElement) {
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

    .badge-preferred {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .badge-type {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }
  `;

  static properties = {
    _supplier: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._supplier = {
      name: '',
      code: '',
      type: 'Manufacturer',
      description: '',
      isActive: true,
      isPreferred: false,
      contactName: '',
      email: '',
      phone: '',
      website: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      postalCode: '',
      country: '',
      taxId: '',
      paymentTerms: '',
      currencyCode: 'USD',
      leadTimeDays: null,
      minimumOrderQuantity: null,
      minOrderValue: null,
      rating: null,
      notes: ''
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadSupplier();
  }

  async _loadSupplier() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/supplier\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const supplierId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplierId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._supplier = await response.json();
        }
      } catch (error) {
        console.error('Error loading supplier:', error);
      }
    }
  }

  getSupplier() {
    return this._supplier;
  }

  setSupplier(supplier) {
    this._supplier = { ...supplier };
    this.requestUpdate();
  }

  isNewSupplier() {
    return this._isNew;
  }

  _getTypeLabel(type) {
    const types = {
      'Manufacturer': 'Manufacturer',
      'Distributor': 'Distributor',
      'Wholesaler': 'Wholesaler',
      'DropShipper': 'Drop Shipper',
      'Importer': 'Importer',
      'Other': 'Other'
    };
    return types[type] || type;
  }

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-truck"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Supplier' : this._supplier.name || 'Supplier'}</h1>
            <div class="header-meta">
              <span class="status-badge ${this._supplier.isActive ? 'status-active' : 'status-inactive'}">
                ${this._supplier.isActive ? 'Active' : 'Inactive'}
              </span>
              ${this._supplier.isPreferred ? html`
                <span class="status-badge badge-preferred">Preferred</span>
              ` : ''}
              <span class="status-badge badge-type">${this._getTypeLabel(this._supplier.type)}</span>
            </div>
          </div>
        </div>
        <slot name="action-menu"></slot>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-supplier-workspace', SupplierWorkspace);

export default SupplierWorkspace;
