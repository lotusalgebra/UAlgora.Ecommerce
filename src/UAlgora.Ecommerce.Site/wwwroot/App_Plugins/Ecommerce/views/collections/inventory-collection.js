import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

/**
 * Inventory Collection
 * Dashboard view for inventory management.
 */
export class InventoryCollection extends UmbElementMixin(LitElement) {
  #authContext;

  async _getAuthHeaders() {
    if (!this.#authContext) {
      this.#authContext = await this.getContext(UMB_AUTH_CONTEXT);
    }
    const token = await this.#authContext?.getLatestToken();
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  async _authFetch(url, options = {}) {
    const headers = await this._getAuthHeaders();
    return fetch(url, {
      ...options,
      headers: { ...headers, ...options.headers }
    });
  }

  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .dashboard-header h1 {
      margin: 0;
      font-size: var(--uui-type-h3-size);
    }

    .quick-actions {
      display: flex;
      gap: var(--uui-size-space-3);
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .stat-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      cursor: pointer;
      transition: box-shadow 0.2s;
    }

    .stat-card:hover {
      box-shadow: var(--uui-shadow-depth-1);
    }

    .stat-card-header {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-space-3);
      color: var(--uui-color-text-alt);
    }

    .stat-card-value {
      font-size: var(--uui-type-h2-size);
      font-weight: 600;
    }

    .stat-card-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .stat-card.alert .stat-card-value {
      color: var(--uui-color-danger);
    }

    .content-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--uui-size-layout-1);
    }

    .list-item {
      display: flex;
      align-items: center;
      padding: var(--uui-size-space-3);
      border-bottom: 1px solid var(--uui-color-border);
      cursor: pointer;
    }

    .list-item:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .list-item:last-child {
      border-bottom: none;
    }

    .list-item-info {
      flex: 1;
    }

    .list-item-title {
      font-weight: 500;
    }

    .list-item-meta {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .status-badge {
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-low { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }
    .status-out { background: var(--uui-color-danger-emphasis); color: var(--uui-color-danger); }
    .status-pending { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-2);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _stats: { type: Object, state: true },
    _lowStockItems: { type: Array, state: true },
    _recentActivity: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._stats = {
      totalProducts: 0,
      totalWarehouses: 0,
      lowStockItems: 0,
      outOfStockItems: 0,
      pendingPurchaseOrders: 0,
      pendingTransfers: 0
    };
    this._lowStockItems = [];
    this._recentActivity = [];
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    try {
      const [statsRes, lowStockRes, activityRes] = await Promise.all([
        this._authFetch('/umbraco/management/api/v1/ecommerce/inventory/overview'),
        this._authFetch('/umbraco/management/api/v1/ecommerce/inventory/low-stock?limit=5'),
        this._authFetch('/umbraco/management/api/v1/ecommerce/inventory/recent-activity?limit=5')
      ]);

      if (statsRes.ok) this._stats = await statsRes.json();
      if (lowStockRes.ok) this._lowStockItems = await lowStockRes.json();
      if (activityRes.ok) this._recentActivity = await activityRes.json();
    } catch (error) {
      console.error('Error loading inventory data:', error);
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
      <div class="dashboard-header">
        <h1>Inventory Management</h1>
        <div class="quick-actions">
          <uui-button look="primary" @click=${() => this._navigate('warehouse/create')}>
            <uui-icon name="icon-add"></uui-icon>
            New Warehouse
          </uui-button>
          <uui-button look="secondary" @click=${() => this._navigate('stock-adjustment/create')}>
            Stock Adjustment
          </uui-button>
          <uui-button look="secondary" @click=${() => this._navigate('stock-transfer/create')}>
            Stock Transfer
          </uui-button>
          <uui-button look="secondary" @click=${() => this._navigate('purchase-order/create')}>
            Purchase Order
          </uui-button>
        </div>
      </div>

      <div class="stats-grid">
        <div class="stat-card" @click=${() => this._navigate('warehouses')}>
          <div class="stat-card-header">
            <uui-icon name="icon-box-open"></uui-icon>
            Warehouses
          </div>
          <div class="stat-card-value">${this._stats.totalWarehouses}</div>
          <div class="stat-card-label">Active locations</div>
        </div>

        <div class="stat-card alert" @click=${() => this._navigate('low-stock')}>
          <div class="stat-card-header">
            <uui-icon name="icon-alert"></uui-icon>
            Low Stock
          </div>
          <div class="stat-card-value">${this._stats.lowStockItems}</div>
          <div class="stat-card-label">Items need attention</div>
        </div>

        <div class="stat-card" @click=${() => this._navigate('purchase-orders?status=pending')}>
          <div class="stat-card-header">
            <uui-icon name="icon-receipt-dollar"></uui-icon>
            Pending Orders
          </div>
          <div class="stat-card-value">${this._stats.pendingPurchaseOrders}</div>
          <div class="stat-card-label">Purchase orders pending</div>
        </div>

        <div class="stat-card" @click=${() => this._navigate('stock-transfers?status=pending')}>
          <div class="stat-card-header">
            <uui-icon name="icon-split"></uui-icon>
            In Transit
          </div>
          <div class="stat-card-value">${this._stats.pendingTransfers}</div>
          <div class="stat-card-label">Transfers in progress</div>
        </div>
      </div>

      <div class="content-grid">
        <uui-box headline="Low Stock Items">
          ${this._lowStockItems.length === 0 ? html`
            <div class="empty-state">
              <p>No low stock items</p>
            </div>
          ` : this._lowStockItems.map(item => html`
            <div class="list-item" @click=${() => this._navigate(`product/edit/${item.productId}`)}>
              <div class="list-item-info">
                <div class="list-item-title">${item.productName}</div>
                <div class="list-item-meta">${item.sku} • ${item.warehouseName}</div>
              </div>
              <span class="status-badge ${item.quantity === 0 ? 'status-out' : 'status-low'}">
                ${item.quantity === 0 ? 'Out of Stock' : `${item.quantity} left`}
              </span>
            </div>
          `)}
          ${this._lowStockItems.length > 0 ? html`
            <div style="padding: var(--uui-size-space-3); text-align: center;">
              <uui-button look="secondary" @click=${() => this._navigate('low-stock')}>
                View All Low Stock Items
              </uui-button>
            </div>
          ` : ''}
        </uui-box>

        <uui-box headline="Recent Activity">
          ${this._recentActivity.length === 0 ? html`
            <div class="empty-state">
              <p>No recent activity</p>
            </div>
          ` : this._recentActivity.map(activity => html`
            <div class="list-item">
              <div class="list-item-info">
                <div class="list-item-title">${activity.description}</div>
                <div class="list-item-meta">${activity.user} • ${activity.timeAgo}</div>
              </div>
            </div>
          `)}
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-inventory-collection', InventoryCollection);

export default InventoryCollection;
