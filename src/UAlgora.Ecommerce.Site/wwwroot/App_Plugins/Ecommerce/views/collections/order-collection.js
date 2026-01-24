import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class OrderCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: 20px;
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      gap: 16px;
      flex-wrap: wrap;
    }

    .filter-group {
      display: flex;
      gap: 8px;
      align-items: center;
    }

    .status-filters {
      display: flex;
      gap: 4px;
      flex-wrap: wrap;
    }

    .status-filter {
      padding: 6px 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
      cursor: pointer;
      font-size: 13px;
      background: #fff;
    }

    .status-filter:hover { background: #f5f5f5; }
    .status-filter.active { background: #1b264f; color: white; border-color: #1b264f; }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: #fff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
    }

    .collection-table th, .collection-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    .collection-table th {
      background: #f5f5f5;
      font-weight: 600;
    }

    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .collection-table tr:last-child td { border-bottom: none; }

    .order-number { font-weight: 500; font-family: monospace; }

    .status-badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 4px;
      font-size: 12px;
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
      padding: 60px 20px;
      color: #666;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 40px;
    }

    .pagination {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 16px;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .total-cell { font-weight: 500; text-align: right; }

    /* Modal Styles */
    .modal-overlay {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }

    .modal {
      background: white;
      border-radius: 8px;
      width: 90%;
      max-width: 800px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .modal-header h2 { margin: 0; font-size: 20px; }

    .modal-body { padding: 20px; }

    .modal-footer {
      padding: 20px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 10px;
    }

    .order-detail-section {
      margin-bottom: 24px;
    }

    .order-detail-section h3 {
      font-size: 16px;
      margin: 0 0 12px 0;
      padding-bottom: 8px;
      border-bottom: 1px solid #eee;
    }

    .detail-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 12px;
    }

    .detail-item {
      display: flex;
      flex-direction: column;
    }

    .detail-label {
      font-size: 12px;
      color: #666;
      margin-bottom: 4px;
    }

    .detail-value {
      font-weight: 500;
    }

    .order-items-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 14px;
    }

    .order-items-table th, .order-items-table td {
      padding: 8px 12px;
      text-align: left;
      border-bottom: 1px solid #eee;
    }

    .order-items-table th { background: #f9f9f9; font-weight: 600; }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group label {
      display: block;
      margin-bottom: 6px;
      font-weight: 500;
    }

    .form-group select {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
    }

    .form-group textarea {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
      min-height: 60px;
      box-sizing: border-box;
    }

    .totals-section {
      text-align: right;
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid #eee;
    }

    .total-line {
      display: flex;
      justify-content: flex-end;
      gap: 24px;
      margin-bottom: 8px;
    }

    .total-line.grand {
      font-size: 18px;
      font-weight: bold;
      padding-top: 8px;
      border-top: 2px solid #333;
    }
  `;

  static properties = {
    _orders: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _statusFilter: { type: String, state: true },
    _page: { type: Number, state: true },
    _pageSize: { type: Number, state: true },
    _totalCount: { type: Number, state: true },
    _showModal: { type: Boolean, state: true },
    _selectedOrder: { type: Object, state: true },
    _loadingOrder: { type: Boolean, state: true },
    _saving: { type: Boolean, state: true },
    _newStatus: { type: String, state: true },
    _statusNote: { type: String, state: true }
  };

  constructor() {
    super();
    this._orders = [];
    this._loading = true;
    this._statusFilter = null;
    this._page = 1;
    this._pageSize = 25;
    this._totalCount = 0;
    this._showModal = false;
    this._selectedOrder = null;
    this._loadingOrder = false;
    this._saving = false;
    this._newStatus = '';
    this._statusNote = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._parseUrlParams();
    this._loadOrders();
  }

  _parseUrlParams() {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');
    if (status) this._statusFilter = status;
  }

  async _loadOrders() {
    try {
      this._loading = true;

      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({
        skip: skip.toString(),
        take: this._pageSize.toString()
      });

      if (this._statusFilter) params.append('status', this._statusFilter);

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order?${params}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) throw new Error('Failed to load orders');

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

  async _openOrderModal(order) {
    this._selectedOrder = order;
    this._newStatus = order.status;
    this._statusNote = '';
    this._showModal = true;

    // Load full order details
    try {
      this._loadingOrder = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._selectedOrder = await response.json();
        this._newStatus = this._selectedOrder.status;
      }
    } catch (error) {
      console.error('Error loading order details:', error);
    } finally {
      this._loadingOrder = false;
    }
  }

  _closeModal() {
    this._showModal = false;
    this._selectedOrder = null;
  }

  async _updateOrderStatus() {
    if (!this._newStatus || this._newStatus === this._selectedOrder.status) {
      this._closeModal();
      return;
    }

    this._saving = true;
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._selectedOrder.id}/status`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify({
          status: this._newStatus,
          note: this._statusNote
        })
      });

      if (!response.ok) throw new Error('Failed to update order status');

      this._closeModal();
      this._loadOrders();
    } catch (error) {
      console.error('Error updating order status:', error);
      alert('Failed to update order status: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  _handlePageChange(direction) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    if (direction === 'prev' && this._page > 1) { this._page--; this._loadOrders(); }
    else if (direction === 'next' && this._page < totalPages) { this._page++; this._loadOrders(); }
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: currencyCode }).format(amount || 0);
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
              <button class="status-filter ${this._statusFilter === status ? 'active' : ''}" @click=${() => this._handleStatusFilter(status)}>
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

      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-receipt-dollar" style="font-size: 48px;"></uui-icon>
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
            <th style="width: 80px;">Actions</th>
          </tr>
        </thead>
        <tbody>
          ${this._orders.map(order => html`
            <tr>
              <td><span class="order-number">#${order.orderNumber}</span></td>
              <td>${new Date(order.createdAt).toLocaleDateString()}</td>
              <td>
                <div>${order.customerName || 'Guest'}</div>
                <div style="font-size: 12px; color: #666;">${order.customerEmail}</div>
              </td>
              <td><span class="status-badge ${this._getStatusClass(order.status)}">${order.status}</span></td>
              <td>${order.itemCount}</td>
              <td class="total-cell">${this._formatCurrency(order.grandTotal, order.currencyCode)}</td>
              <td>
                <uui-button look="secondary" compact label="View" @click=${() => this._openOrderModal(order)}>View</uui-button>
              </td>
            </tr>
          `)}
        </tbody>
      </table>

      <div class="pagination">
        <div>Showing ${startItem}-${endItem} of ${this._totalCount} orders</div>
        <div style="display: flex; gap: 8px;">
          <uui-button look="secondary" compact label="Previous" ?disabled=${this._page === 1} @click=${() => this._handlePageChange('prev')}>Previous</uui-button>
          <uui-button look="secondary" compact label="Next" ?disabled=${this._page >= totalPages} @click=${() => this._handlePageChange('next')}>Next</uui-button>
        </div>
      </div>
    `;
  }

  _renderModal() {
    const order = this._selectedOrder;
    const statuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled', 'Refunded', 'OnHold', 'Failed'];

    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header">
            <h2>Order #${order?.orderNumber}</h2>
            <uui-button look="secondary" compact label="Close" @click=${this._closeModal}>&times;</uui-button>
          </div>
          <div class="modal-body">
            ${this._loadingOrder ? html`<div class="loading"><uui-loader></uui-loader></div>` : html`
              <div class="order-detail-section">
                <h3>Order Information</h3>
                <div class="detail-grid">
                  <div class="detail-item">
                    <span class="detail-label">Order Number</span>
                    <span class="detail-value">#${order?.orderNumber}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Date</span>
                    <span class="detail-value">${order?.createdAt ? new Date(order.createdAt).toLocaleString() : '-'}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Customer</span>
                    <span class="detail-value">${order?.customerName || 'Guest'}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Email</span>
                    <span class="detail-value">${order?.customerEmail || '-'}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Phone</span>
                    <span class="detail-value">${order?.customerPhone || '-'}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Current Status</span>
                    <span class="status-badge ${this._getStatusClass(order?.status)}">${order?.status}</span>
                  </div>
                </div>
              </div>

              ${order?.shippingAddress ? html`
                <div class="order-detail-section">
                  <h3>Shipping Address</h3>
                  <div class="detail-value">
                    ${order.shippingAddress.firstName} ${order.shippingAddress.lastName}<br>
                    ${order.shippingAddress.address1}<br>
                    ${order.shippingAddress.address2 ? html`${order.shippingAddress.address2}<br>` : ''}
                    ${order.shippingAddress.city}, ${order.shippingAddress.stateProvince} ${order.shippingAddress.postalCode}<br>
                    ${order.shippingAddress.country}
                  </div>
                </div>
              ` : ''}

              <div class="order-detail-section">
                <h3>Order Items</h3>
                <table class="order-items-table">
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>SKU</th>
                      <th>Qty</th>
                      <th style="text-align: right;">Price</th>
                      <th style="text-align: right;">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${(order?.items || []).map(item => html`
                      <tr>
                        <td>${item.productName}</td>
                        <td>${item.sku || '-'}</td>
                        <td>${item.quantity}</td>
                        <td style="text-align: right;">${this._formatCurrency(item.unitPrice, order?.currencyCode)}</td>
                        <td style="text-align: right;">${this._formatCurrency(item.totalPrice, order?.currencyCode)}</td>
                      </tr>
                    `)}
                  </tbody>
                </table>

                <div class="totals-section">
                  <div class="total-line">
                    <span>Subtotal:</span>
                    <span>${this._formatCurrency(order?.subtotal, order?.currencyCode)}</span>
                  </div>
                  ${order?.shippingTotal ? html`
                    <div class="total-line">
                      <span>Shipping:</span>
                      <span>${this._formatCurrency(order?.shippingTotal, order?.currencyCode)}</span>
                    </div>
                  ` : ''}
                  ${order?.taxTotal ? html`
                    <div class="total-line">
                      <span>Tax:</span>
                      <span>${this._formatCurrency(order?.taxTotal, order?.currencyCode)}</span>
                    </div>
                  ` : ''}
                  ${order?.discountTotal ? html`
                    <div class="total-line">
                      <span>Discount:</span>
                      <span>-${this._formatCurrency(order?.discountTotal, order?.currencyCode)}</span>
                    </div>
                  ` : ''}
                  <div class="total-line grand">
                    <span>Grand Total:</span>
                    <span>${this._formatCurrency(order?.grandTotal, order?.currencyCode)}</span>
                  </div>
                </div>
              </div>

              <div class="order-detail-section">
                <h3>Update Status</h3>
                <div class="form-group">
                  <label>New Status</label>
                  <select .value=${this._newStatus} @change=${(e) => this._newStatus = e.target.value}>
                    ${statuses.map(s => html`<option value="${s}" ?selected=${this._newStatus === s}>${s}</option>`)}
                  </select>
                </div>
                <div class="form-group">
                  <label>Note (optional)</label>
                  <textarea .value=${this._statusNote} @input=${(e) => this._statusNote = e.target.value} placeholder="Add a note about this status change..."></textarea>
                </div>
              </div>
            `}
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" label="Close" @click=${this._closeModal} ?disabled=${this._saving}>Close</uui-button>
            <uui-button look="primary" label="Update Status" @click=${this._updateOrderStatus} ?disabled=${this._saving || this._loadingOrder}>
              ${this._saving ? 'Updating...' : 'Update Status'}
            </uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-collection', OrderCollection);
export default OrderCollection;
