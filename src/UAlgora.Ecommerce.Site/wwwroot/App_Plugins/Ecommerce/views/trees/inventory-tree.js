import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Inventory Tree
 * Displays the inventory management tree structure.
 */
export class InventoryTree extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
    }

    .tree-container {
      padding: var(--uui-size-space-3);
    }

    .tree-section {
      margin-bottom: var(--uui-size-space-4);
    }

    .tree-section-header {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-2);
      font-weight: 500;
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .tree-item {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-3);
      cursor: pointer;
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-1);
    }

    .tree-item:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .tree-item.active {
      background: var(--uui-color-selected);
    }

    .tree-item uui-icon {
      color: var(--uui-color-interactive);
    }

    .tree-item-label {
      flex: 1;
    }

    .tree-item-count {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      background: var(--uui-color-surface-alt);
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
    }
  `;

  static properties = {
    _counts: { type: Object, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._counts = {
      warehouses: 0,
      suppliers: 0,
      purchaseOrders: 0,
      stockAdjustments: 0,
      stockTransfers: 0,
      lowStock: 0
    };
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCounts();
  }

  async _loadCounts() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/inventory/stats', {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._counts = await response.json();
      }
    } catch (error) {
      console.error('Error loading inventory stats:', error);
    } finally {
      this._loading = false;
    }
  }

  _navigate(path) {
    window.history.pushState({}, '', `/umbraco/section/ecommerce/${path}`);
    window.dispatchEvent(new PopStateEvent('popstate'));
  }

  render() {
    return html`
      <div class="tree-container">
        <div class="tree-section">
          <div class="tree-section-header">
            <uui-icon name="icon-box-open"></uui-icon>
            Inventory Management
          </div>

          <div class="tree-item" @click=${() => this._navigate('warehouses')}>
            <uui-icon name="icon-box-open"></uui-icon>
            <span class="tree-item-label">Warehouses</span>
            <span class="tree-item-count">${this._counts.warehouses}</span>
          </div>

          <div class="tree-item" @click=${() => this._navigate('low-stock')}>
            <uui-icon name="icon-alert"></uui-icon>
            <span class="tree-item-label">Low Stock Items</span>
            <span class="tree-item-count">${this._counts.lowStock}</span>
          </div>
        </div>

        <div class="tree-section">
          <div class="tree-section-header">
            <uui-icon name="icon-truck"></uui-icon>
            Suppliers
          </div>

          <div class="tree-item" @click=${() => this._navigate('suppliers')}>
            <uui-icon name="icon-truck"></uui-icon>
            <span class="tree-item-label">All Suppliers</span>
            <span class="tree-item-count">${this._counts.suppliers}</span>
          </div>

          <div class="tree-item" @click=${() => this._navigate('purchase-orders')}>
            <uui-icon name="icon-receipt-dollar"></uui-icon>
            <span class="tree-item-label">Purchase Orders</span>
            <span class="tree-item-count">${this._counts.purchaseOrders}</span>
          </div>
        </div>

        <div class="tree-section">
          <div class="tree-section-header">
            <uui-icon name="icon-axis-rotation"></uui-icon>
            Operations
          </div>

          <div class="tree-item" @click=${() => this._navigate('stock-adjustments')}>
            <uui-icon name="icon-axis-rotation"></uui-icon>
            <span class="tree-item-label">Stock Adjustments</span>
            <span class="tree-item-count">${this._counts.stockAdjustments}</span>
          </div>

          <div class="tree-item" @click=${() => this._navigate('stock-transfers')}>
            <uui-icon name="icon-split"></uui-icon>
            <span class="tree-item-label">Stock Transfers</span>
            <span class="tree-item-count">${this._counts.stockTransfers}</span>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-inventory-tree', InventoryTree);

export default InventoryTree;
