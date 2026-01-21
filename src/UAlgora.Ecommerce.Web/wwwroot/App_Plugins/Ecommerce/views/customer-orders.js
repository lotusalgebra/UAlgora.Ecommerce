import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Orders View
 * Displays order history for the customer.
 */
export class CustomerOrders extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .orders-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .stats-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .stat-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      text-align: center;
    }

    .stat-value {
      font-size: var(--uui-type-h2-size);
      font-weight: bold;
      color: var(--uui-color-interactive);
      margin-bottom: var(--uui-size-space-2);
    }

    .stat-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .orders-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .orders-table th,
    .orders-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .orders-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .orders-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
    }

    .orders-table tr:last-child td {
      border-bottom: none;
    }

    .order-number {
      font-weight: 500;
      font-family: monospace;
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
      text-transform: uppercase;
    }

    .status-pending { background: #ffc107; color: #000; }
    .status-confirmed { background: #17a2b8; color: #fff; }
    .status-processing { background: #007bff; color: #fff; }
    .status-shipped { background: #6f42c1; color: #fff; }
    .status-delivered { background: #28a745; color: #fff; }
    .status-completed { background: #28a745; color: #fff; }
    .status-cancelled { background: #dc3545; color: #fff; }
    .status-refunded { background: #fd7e14; color: #fff; }

    .total-cell {
      font-weight: 500;
      text-align: right;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }

    .pagination {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: var(--uui-size-layout-1);
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .pagination-info {
      color: var(--uui-color-text-alt);
    }

    .pagination-buttons {
      display: flex;
      gap: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _customer: { type: Object, state: true },
    _orders: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._customer = null;
    this._orders = [];
    this._loading = true;
    this._page = 1;
    this._pageSize = 10;
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      this._customer = workspace.getCustomer();
      if (this._customer?.id) {
        this._loadOrders();
      } else {
        this._loading = false;
      }
    }
  }

  async _loadOrders() {
    try {
      this._loading = true;

      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({
        skip: skip.toString(),
        take: this._pageSize.toString()
      });

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${this._customer.id}/orders?${params}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load orders');
      }

      const data = await response.json();
      this._orders = data.items || [];
      this._totalCount = data.total || this._orders.length;
    } catch (error) {
      console.error('Error loading orders:', error);
      this._orders = [];
    } finally {
      this._loading = false;
    }
  }

  _handlePageChange(direction) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);

    if (direction === 'prev' && this._page > 1) {
      this._page--;
      this._loadOrders();
    } else if (direction === 'next' && this._page < totalPages) {
      this._page++;
      this._loadOrders();
    }
  }

  _handleOrderClick(order) {
    window.history.pushState(null, '', `/umbraco/section/ecommerce/workspace/order/edit/${order.id}`);

    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: {
        path: `/section/ecommerce/workspace/order/edit/${order.id}`
      }
    });
    this.dispatchEvent(event);
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode
    }).format(amount || 0);
  }

  _getStatusClass(status) {
    return `status-${status?.toLowerCase().replace(/\s+/g, '') || 'pending'}`;
  }

  render() {
    const isNewCustomer = !this._customer?.id;

    if (isNewCustomer) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-receipt-dollar"></uui-icon>
          <h3>No orders yet</h3>
          <p>Save the customer first to view their order history.</p>
        </div>
      `;
    }

    return html`
      <div class="stats-row">
        <div class="stat-card">
          <div class="stat-value">${this._customer?.totalOrders || 0}</div>
          <div class="stat-label">Total Orders</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">${this._formatCurrency(this._customer?.totalSpent)}</div>
          <div class="stat-label">Total Spent</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">${this._formatCurrency(this._customer?.averageOrderValue)}</div>
          <div class="stat-label">Avg Order Value</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">
            ${this._customer?.lastOrderAt
              ? new Date(this._customer.lastOrderAt).toLocaleDateString()
              : 'Never'}
          </div>
          <div class="stat-label">Last Order</div>
        </div>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._orders.length === 0
          ? this._renderEmptyState()
          : this._renderOrdersTable()
      }
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-receipt-dollar"></uui-icon>
        <h3>No orders found</h3>
        <p>This customer hasn't placed any orders yet.</p>
      </div>
    `;
  }

  _renderOrdersTable() {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    const startItem = ((this._page - 1) * this._pageSize) + 1;
    const endItem = Math.min(this._page * this._pageSize, this._totalCount);

    return html`
      <table class="orders-table">
        <thead>
          <tr>
            <th>Order</th>
            <th>Date</th>
            <th>Status</th>
            <th>Items</th>
            <th class="total-cell">Total</th>
          </tr>
        </thead>
        <tbody>
          ${this._orders.map(order => html`
            <tr @click=${() => this._handleOrderClick(order)}>
              <td>
                <span class="order-number">#${order.orderNumber}</span>
              </td>
              <td>
                ${new Date(order.createdAt).toLocaleDateString()}
              </td>
              <td>
                <span class="status-badge ${this._getStatusClass(order.status)}">
                  ${order.status}
                </span>
              </td>
              <td>${order.itemCount}</td>
              <td class="total-cell">
                ${this._formatCurrency(order.grandTotal, order.currencyCode)}
              </td>
            </tr>
          `)}
        </tbody>
      </table>

      ${this._totalCount > this._pageSize ? html`
        <div class="pagination">
          <div class="pagination-info">
            Showing ${startItem}-${endItem} of ${this._totalCount} orders
          </div>
          <div class="pagination-buttons">
            <uui-button
              look="secondary"
              compact
              ?disabled=${this._page === 1}
              @click=${() => this._handlePageChange('prev')}
            >
              Previous
            </uui-button>
            <uui-button
              look="secondary"
              compact
              ?disabled=${this._page >= totalPages}
              @click=${() => this._handlePageChange('next')}
            >
              Next
            </uui-button>
          </div>
        </div>
      ` : ''}
    `;
  }
}

customElements.define('ecommerce-customer-orders', CustomerOrders);

export default CustomerOrders;
