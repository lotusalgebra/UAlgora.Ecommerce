import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Stock Levels
 * View and manage stock levels in this warehouse.
 */
export class WarehouseStock extends UmbElementMixin(LitElement) {
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

    .search-box {
      display: flex;
      gap: var(--uui-size-space-3);
    }

    uui-input {
      width: 300px;
    }

    .stock-table {
      width: 100%;
      border-collapse: collapse;
    }

    .stock-table th,
    .stock-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .stock-table th {
      background: var(--uui-color-surface-alt);
      font-weight: 500;
    }

    .stock-table tbody tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .stock-status {
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-in-stock {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .status-low-stock {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .status-out-of-stock {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
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

    .quantity-cell {
      font-variant-numeric: tabular-nums;
    }

    .reserved {
      color: var(--uui-color-warning);
    }

    .available {
      color: var(--uui-color-positive);
      font-weight: 500;
    }
  `;

  static properties = {
    _stockItems: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _filter: { type: String, state: true }
  };

  constructor() {
    super();
    this._stockItems = [];
    this._loading = true;
    this._searchTerm = '';
    this._filter = 'all';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadStock();
  }

  async _loadStock() {
    const workspace = this.closest('ecommerce-warehouse-workspace');
    if (!workspace) {
      setTimeout(() => this._loadStock(), 100);
      return;
    }

    const warehouse = workspace.getWarehouse();
    if (!warehouse.id) {
      this._loading = false;
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}/stock`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._stockItems = await response.json();
      }
    } catch (error) {
      console.error('Error loading stock:', error);
    } finally {
      this._loading = false;
    }
  }

  _getStockStatus(item) {
    const available = item.quantityOnHand - item.quantityReserved;
    if (available <= 0) return 'out-of-stock';
    if (item.lowStockThreshold && available <= item.lowStockThreshold) return 'low-stock';
    return 'in-stock';
  }

  _getStatusLabel(status) {
    const labels = {
      'in-stock': 'In Stock',
      'low-stock': 'Low Stock',
      'out-of-stock': 'Out of Stock'
    };
    return labels[status] || status;
  }

  get _filteredItems() {
    let items = this._stockItems;

    if (this._searchTerm) {
      const term = this._searchTerm.toLowerCase();
      items = items.filter(item =>
        item.productName?.toLowerCase().includes(term) ||
        item.sku?.toLowerCase().includes(term)
      );
    }

    if (this._filter !== 'all') {
      items = items.filter(item => this._getStockStatus(item) === this._filter);
    }

    return items;
  }

  render() {
    if (this._loading) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <div class="toolbar">
          <div class="search-box">
            <uui-input
              placeholder="Search products..."
              @input=${(e) => this._searchTerm = e.target.value}
            >
              <uui-icon name="icon-search" slot="prepend"></uui-icon>
            </uui-input>
            <uui-select @change=${(e) => this._filter = e.target.value}>
              <option value="all">All Stock</option>
              <option value="in-stock">In Stock</option>
              <option value="low-stock">Low Stock</option>
              <option value="out-of-stock">Out of Stock</option>
            </uui-select>
          </div>
          <uui-button look="primary" @click=${this._loadStock}>
            <uui-icon name="icon-sync"></uui-icon>
            Refresh
          </uui-button>
        </div>

        ${this._filteredItems.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-box-open"></uui-icon>
            <p>No stock items found</p>
          </div>
        ` : html`
          <table class="stock-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>SKU</th>
                <th>Bin Location</th>
                <th>On Hand</th>
                <th>Reserved</th>
                <th>Available</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              ${this._filteredItems.map(item => {
                const status = this._getStockStatus(item);
                const available = item.quantityOnHand - item.quantityReserved;
                return html`
                  <tr>
                    <td>${item.productName || 'Unknown Product'}</td>
                    <td>${item.sku || '-'}</td>
                    <td>${item.binLocation || '-'}</td>
                    <td class="quantity-cell">${item.quantityOnHand}</td>
                    <td class="quantity-cell reserved">${item.quantityReserved}</td>
                    <td class="quantity-cell available">${available}</td>
                    <td>
                      <span class="stock-status status-${status}">
                        ${this._getStatusLabel(status)}
                      </span>
                    </td>
                  </tr>
                `;
              })}
            </tbody>
          </table>
        `}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-warehouse-stock', WarehouseStock);

export default WarehouseStock;
