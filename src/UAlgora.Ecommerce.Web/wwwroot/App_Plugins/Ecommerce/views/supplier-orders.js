import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Supplier Purchase Orders
 * View purchase orders for this supplier.
 */
export class SupplierOrders extends UmbElementMixin(LitElement) {
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

    .filter-group {
      display: flex;
      gap: var(--uui-size-space-3);
    }

    .orders-table {
      width: 100%;
      border-collapse: collapse;
    }

    .orders-table th,
    .orders-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .orders-table th {
      background: var(--uui-color-surface-alt);
      font-weight: 500;
    }

    .orders-table tbody tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
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
    .status-sent { background: var(--uui-color-default-emphasis); color: var(--uui-color-default); }
    .status-partiallyreceived { background: var(--uui-color-warning-emphasis); color: var(--uui-color-warning); }
    .status-received { background: var(--uui-color-positive-emphasis); color: var(--uui-color-positive); }
    .status-completed { background: var(--uui-color-positive-emphasis); color: var(--uui-color-positive); }
    .status-cancelled { background: var(--uui-color-danger-emphasis); color: var(--uui-color-danger); }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-4);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .amount-cell {
      font-variant-numeric: tabular-nums;
      font-weight: 500;
    }
  `;

  static properties = {
    _orders: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _statusFilter: { type: String, state: true }
  };

  constructor() {
    super();
    this._orders = [];
    this._loading = true;
    this._statusFilter = 'all';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadOrders();
  }

  async _loadOrders() {
    const workspace = this.closest('ecommerce-supplier-workspace');
    if (!workspace) {
      setTimeout(() => this._loadOrders(), 100);
      return;
    }

    const supplier = workspace.getSupplier();
    if (!supplier.id) {
      this._loading = false;
      return;
    }

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/supplier/${supplier.id}/purchase-orders`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._orders = await response.json();
      }
    } catch (error) {
      console.error('Error loading purchase orders:', error);
    } finally {
      this._loading = false;
    }
  }

  get _filteredOrders() {
    if (this._statusFilter === 'all') return this._orders;
    return this._orders.filter(o => o.status?.toLowerCase() === this._statusFilter);
  }

  _formatCurrency(amount) {
    if (amount == null) return '-';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  _formatDate(date) {
    if (!date) return '-';
    return new Date(date).toLocaleDateString();
  }

  _getStatusLabel(status) {
    const labels = {
      'Draft': 'Draft',
      'Pending': 'Pending Approval',
      'Approved': 'Approved',
      'Sent': 'Sent to Supplier',
      'PartiallyReceived': 'Partially Received',
      'Received': 'Received',
      'Completed': 'Completed',
      'Cancelled': 'Cancelled'
    };
    return labels[status] || status;
  }

  render() {
    if (this._loading) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <div class="toolbar">
          <div class="filter-group">
            <uui-select @change=${(e) => this._statusFilter = e.target.value}>
              <option value="all">All Orders</option>
              <option value="draft">Draft</option>
              <option value="pending">Pending</option>
              <option value="approved">Approved</option>
              <option value="sent">Sent</option>
              <option value="partiallyreceived">Partially Received</option>
              <option value="received">Received</option>
              <option value="completed">Completed</option>
              <option value="cancelled">Cancelled</option>
            </uui-select>
          </div>
          <uui-button look="primary">
            <uui-icon name="icon-add"></uui-icon>
            New Purchase Order
          </uui-button>
        </div>

        ${this._filteredOrders.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-receipt-dollar"></uui-icon>
            <p>No purchase orders found</p>
          </div>
        ` : html`
          <table class="orders-table">
            <thead>
              <tr>
                <th>Order #</th>
                <th>Date</th>
                <th>Warehouse</th>
                <th>Items</th>
                <th>Total</th>
                <th>Status</th>
                <th>Expected</th>
              </tr>
            </thead>
            <tbody>
              ${this._filteredOrders.map(order => html`
                <tr @click=${() => this._openOrder(order.id)}>
                  <td>${order.orderNumber}</td>
                  <td>${this._formatDate(order.orderDate)}</td>
                  <td>${order.warehouseName || '-'}</td>
                  <td>${order.itemCount || 0}</td>
                  <td class="amount-cell">${this._formatCurrency(order.total)}</td>
                  <td>
                    <span class="status-badge status-${order.status?.toLowerCase()}">
                      ${this._getStatusLabel(order.status)}
                    </span>
                  </td>
                  <td>${this._formatDate(order.expectedDeliveryDate)}</td>
                </tr>
              `)}
            </tbody>
          </table>
        `}
      </uui-box>
    `;
  }

  _openOrder(orderId) {
    // Navigate to purchase order editor
    window.history.pushState({}, '', `/umbraco/section/ecommerce/purchase-order/edit/${orderId}`);
    window.dispatchEvent(new PopStateEvent('popstate'));
  }
}

customElements.define('ecommerce-supplier-orders', SupplierOrders);

export default SupplierOrders;
