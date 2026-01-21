import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Collection View
 * Displays a list of orders with filtering by status.
 */
export class OrderCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
      gap: var(--uui-size-layout-1);
      flex-wrap: wrap;
    }

    .filter-group {
      display: flex;
      gap: var(--uui-size-space-4);
      align-items: center;
    }

    .status-filters {
      display: flex;
      gap: var(--uui-size-space-2);
      flex-wrap: wrap;
    }

    .status-filter {
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      font-size: var(--uui-type-small-size);
      background: var(--uui-color-surface);
    }

    .status-filter:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .status-filter.active {
      background: var(--uui-color-selected);
      border-color: var(--uui-color-selected);
    }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .collection-table th,
    .collection-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .collection-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .collection-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
    }

    .collection-table tr:last-child td {
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
    .status-onhold { background: #6c757d; color: #fff; }
    .status-failed { background: #dc3545; color: #fff; }

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

    .total-cell {
      font-weight: 500;
      text-align: right;
    }

    .date-cell {
      white-space: nowrap;
    }
  `;

  static properties = {
    _orders: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _statusFilter: { type: String, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._orders = [];
    this._loading = true;
    this._statusFilter = null;
    this._page = 1;
    this._pageSize = 25;
    this._totalCount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._parseUrlParams();
    this._loadOrders();
  }

  _parseUrlParams() {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');
    if (status) {
      this._statusFilter = status;
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

      if (this._statusFilter) {
        params.append('status', this._statusFilter);
      }

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order?${params}`, {
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

  _handleStatusFilter(status) {
    this._statusFilter = status === this._statusFilter ? null : status;
    this._page = 1;
    this._loadOrders();
  }

  _handleRowClick(order) {
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
    const statuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled'];

    return html`
      <div class="collection-header">
        <div class="filter-group">
          <div class="status-filters">
            ${statuses.map(status => html`
              <button
                class="status-filter ${this._statusFilter === status ? 'active' : ''}"
                @click=${() => this._handleStatusFilter(status)}
              >
                ${status}
              </button>
            `)}
          </div>
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
        <p>${this._statusFilter ? `No ${this._statusFilter.toLowerCase()} orders` : 'No orders yet'}</p>
      </div>
    `;
  }

  _renderOrdersTable() {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    const startItem = ((this._page - 1) * this._pageSize) + 1;
    const endItem = Math.min(this._page * this._pageSize, this._totalCount);

    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th>Order</th>
            <th>Date</th>
            <th>Customer</th>
            <th>Status</th>
            <th>Items</th>
            <th class="total-cell">Total</th>
          </tr>
        </thead>
        <tbody>
          ${this._orders.map(order => html`
            <tr @click=${() => this._handleRowClick(order)}>
              <td>
                <span class="order-number">#${order.orderNumber}</span>
              </td>
              <td class="date-cell">
                ${new Date(order.createdAt).toLocaleDateString()}
              </td>
              <td>
                <div>${order.customerName || 'Guest'}</div>
                <div style="font-size: var(--uui-type-small-size); color: var(--uui-color-text-alt);">
                  ${order.customerEmail}
                </div>
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
    `;
  }
}

customElements.define('ecommerce-order-collection', OrderCollection);

export default OrderCollection;
