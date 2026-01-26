import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class OrderCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; height: 100%; }
    .container { display: flex; height: 100%; }
    .list-panel { width: 420px; border-right: 1px solid #e0e0e0; display: flex; flex-direction: column; background: #fafafa; }
    .editor-panel { flex: 1; overflow-y: auto; background: #fff; }
    .list-header { padding: 16px; border-bottom: 1px solid #e0e0e0; background: #fff; }
    .list-header h2 { margin: 0 0 12px 0; font-size: 18px; }
    .search-row { display: flex; gap: 8px; margin-bottom: 12px; }
    .search-row uui-input { flex: 1; }
    .status-filters { display: flex; gap: 4px; flex-wrap: wrap; }
    .status-filter { padding: 4px 10px; border: 1px solid #ddd; border-radius: 16px; cursor: pointer; font-size: 11px; background: #fff; transition: all 0.2s; }
    .status-filter:hover { background: #f0f0f0; }
    .status-filter.active { background: #1976d2; color: white; border-color: #1976d2; }
    .list-content { flex: 1; overflow-y: auto; }
    .list-item { padding: 14px 16px; border-bottom: 1px solid #e9e9e9; cursor: pointer; display: flex; gap: 12px; }
    .list-item:hover { background: #f5f5f5; }
    .list-item.active { background: #e3f2fd; border-left: 3px solid #1976d2; }
    .order-icon { width: 48px; height: 48px; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 12px; color: white; flex-shrink: 0; }
    .order-icon.pending { background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); }
    .order-icon.confirmed { background: linear-gradient(135deg, #17a2b8 0%, #138496 100%); }
    .order-icon.processing { background: linear-gradient(135deg, #007bff 0%, #0056b3 100%); }
    .order-icon.shipped { background: linear-gradient(135deg, #6f42c1 0%, #5a32a3 100%); }
    .order-icon.delivered, .order-icon.completed { background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%); }
    .order-icon.cancelled, .order-icon.failed { background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); }
    .order-icon.refunded { background: linear-gradient(135deg, #fd7e14 0%, #e96b00 100%); }
    .order-icon.onhold { background: linear-gradient(135deg, #6c757d 0%, #545b62 100%); }
    .order-info { flex: 1; min-width: 0; }
    .order-number { font-weight: 600; font-family: monospace; font-size: 14px; }
    .order-customer { font-size: 13px; color: #333; margin-top: 2px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .order-meta { font-size: 12px; color: #666; margin-top: 4px; display: flex; gap: 12px; align-items: center; }
    .order-total { font-weight: 600; font-size: 14px; text-align: right; }
    .order-date { font-size: 11px; color: #888; }
    .status-dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; }
    .status-dot.pending { background: #ffc107; }
    .status-dot.confirmed { background: #17a2b8; }
    .status-dot.processing { background: #007bff; }
    .status-dot.shipped { background: #6f42c1; }
    .status-dot.delivered, .status-dot.completed { background: #28a745; }
    .status-dot.cancelled, .status-dot.failed { background: #dc3545; }
    .status-dot.refunded { background: #fd7e14; }
    .status-dot.onhold { background: #6c757d; }
    .editor-header { padding: 20px 24px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; background: #fff; position: sticky; top: 0; z-index: 10; }
    .editor-header-left { display: flex; align-items: center; gap: 16px; }
    .editor-header h2 { margin: 0; font-size: 20px; font-family: monospace; }
    .editor-actions { display: flex; gap: 8px; }
    .status-badge { display: inline-block; padding: 6px 12px; border-radius: 4px; font-size: 12px; font-weight: 600; text-transform: uppercase; }
    .status-badge.pending { background: #fff3cd; color: #856404; }
    .status-badge.confirmed { background: #d1ecf1; color: #0c5460; }
    .status-badge.processing { background: #cce5ff; color: #004085; }
    .status-badge.shipped { background: #e2d5f1; color: #4a2d7f; }
    .status-badge.delivered, .status-badge.completed { background: #d4edda; color: #155724; }
    .status-badge.cancelled, .status-badge.failed { background: #f8d7da; color: #721c24; }
    .status-badge.refunded { background: #ffe5d0; color: #a84300; }
    .status-badge.onhold { background: #e2e3e5; color: #383d41; }
    .tabs { display: flex; gap: 4px; border-bottom: 1px solid #e0e0e0; padding: 0 24px; background: #fafafa; }
    .tab { padding: 12px 20px; cursor: pointer; border-bottom: 2px solid transparent; color: #666; font-weight: 500; font-size: 14px; }
    .tab:hover { color: #333; }
    .tab.active { color: #1976d2; border-bottom-color: #1976d2; }
    .tab-content { display: none; }
    .tab-content.active { display: block; }
    .editor-body { padding: 24px; }
    .info-cards { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .info-card { background: #f8f9fa; border-radius: 8px; padding: 16px; }
    .info-card-label { font-size: 12px; color: #666; margin-bottom: 4px; }
    .info-card-value { font-size: 18px; font-weight: 600; }
    .info-card-value.large { font-size: 24px; color: #1976d2; }
    .section { margin-bottom: 24px; }
    .section-title { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 16px; padding-bottom: 8px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; }
    .detail-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
    .detail-item { }
    .detail-label { font-size: 12px; color: #666; margin-bottom: 4px; }
    .detail-value { font-weight: 500; }
    .items-table { width: 100%; border-collapse: collapse; }
    .items-table th, .items-table td { padding: 12px; text-align: left; border-bottom: 1px solid #eee; }
    .items-table th { background: #f8f9fa; font-weight: 600; font-size: 12px; text-transform: uppercase; color: #666; }
    .items-table .product-cell { display: flex; align-items: center; gap: 12px; }
    .product-thumb { width: 48px; height: 48px; border-radius: 6px; background: #f0f0f0; object-fit: cover; }
    .product-name { font-weight: 500; }
    .product-sku { font-size: 12px; color: #666; font-family: monospace; }
    .text-right { text-align: right; }
    .totals-table { width: 300px; margin-left: auto; margin-top: 16px; }
    .totals-table td { padding: 8px 0; }
    .totals-table .total-row { font-size: 18px; font-weight: 700; border-top: 2px solid #333; }
    .address-card { background: #f8f9fa; border-radius: 8px; padding: 16px; }
    .address-card h4 { margin: 0 0 12px 0; font-size: 14px; }
    .address-line { margin-bottom: 4px; }
    .timeline { position: relative; padding-left: 24px; }
    .timeline::before { content: ''; position: absolute; left: 8px; top: 0; bottom: 0; width: 2px; background: #e0e0e0; }
    .timeline-item { position: relative; padding-bottom: 20px; }
    .timeline-item:last-child { padding-bottom: 0; }
    .timeline-dot { position: absolute; left: -20px; top: 4px; width: 12px; height: 12px; border-radius: 50%; background: #1976d2; border: 2px solid #fff; box-shadow: 0 0 0 2px #1976d2; }
    .timeline-dot.completed { background: #28a745; box-shadow: 0 0 0 2px #28a745; }
    .timeline-dot.cancelled { background: #dc3545; box-shadow: 0 0 0 2px #dc3545; }
    .timeline-content { background: #f8f9fa; border-radius: 8px; padding: 12px 16px; }
    .timeline-title { font-weight: 600; margin-bottom: 4px; }
    .timeline-date { font-size: 12px; color: #666; }
    .timeline-note { font-size: 13px; color: #555; margin-top: 8px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; font-size: 13px; }
    .form-group select, .form-group textarea { width: 100%; padding: 10px 12px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; box-sizing: border-box; }
    .form-group textarea { min-height: 80px; resize: vertical; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #666; }
    .empty-state uui-icon { font-size: 64px; margin-bottom: 16px; opacity: 0.5; }
    .loading { display: flex; justify-content: center; align-items: center; height: 100%; }
    .pagination { padding: 12px 16px; border-top: 1px solid #e0e0e0; background: #fff; display: flex; justify-content: space-between; align-items: center; font-size: 13px; }
    .quick-actions { display: flex; gap: 8px; flex-wrap: wrap; }
    .quick-action { padding: 8px 16px; border: 1px solid #ddd; border-radius: 4px; background: #fff; cursor: pointer; font-size: 13px; display: flex; align-items: center; gap: 6px; }
    .quick-action:hover { background: #f5f5f5; }
    .quick-action.primary { background: #1976d2; color: white; border-color: #1976d2; }
    .quick-action.danger { color: #dc3545; border-color: #dc3545; }
    .payment-info { display: flex; align-items: center; gap: 12px; padding: 12px; background: #f8f9fa; border-radius: 8px; }
    .payment-icon { width: 40px; height: 40px; background: #e3f2fd; border-radius: 8px; display: flex; align-items: center; justify-content: center; }
    .payment-details { flex: 1; }
    .payment-method { font-weight: 600; }
    .payment-status { font-size: 12px; color: #666; }
  `;

  static properties = {
    _orders: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _statusFilter: { state: true },
    _page: { state: true },
    _pageSize: { state: true },
    _totalCount: { state: true },
    _selectedOrder: { state: true },
    _loadingOrder: { state: true },
    _activeTab: { state: true },
    _saving: { state: true },
    _newStatus: { state: true },
    _statusNote: { state: true }
  };

  constructor() {
    super();
    this._orders = [];
    this._loading = true;
    this._searchTerm = '';
    this._statusFilter = null;
    this._page = 1;
    this._pageSize = 25;
    this._totalCount = 0;
    this._selectedOrder = null;
    this._loadingOrder = false;
    this._activeTab = 'details';
    this._saving = false;
    this._newStatus = '';
    this._statusNote = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadOrders();
  }

  async _loadOrders() {
    try {
      this._loading = true;
      const skip = (this._page - 1) * this._pageSize;
      const params = new URLSearchParams({ skip: skip.toString(), take: this._pageSize.toString() });
      if (this._statusFilter) params.append('status', this._statusFilter);
      if (this._searchTerm) params.append('search', this._searchTerm);

      const res = await fetch(`/umbraco/management/api/v1/ecommerce/order?${params}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (res.ok) {
        const data = await res.json();
        this._orders = data.items || data || [];
        this._totalCount = data.total || this._orders.length;
      }
    } catch (e) { console.error('Error loading orders:', e); }
    finally { this._loading = false; }
  }

  async _selectOrder(order) {
    this._selectedOrder = order;
    this._newStatus = order.status;
    this._statusNote = '';
    this._activeTab = 'details';

    try {
      this._loadingOrder = true;
      const res = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}`, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (res.ok) {
        this._selectedOrder = await res.json();
        this._newStatus = this._selectedOrder.status;
      }
    } catch (e) { console.error('Error loading order:', e); }
    finally { this._loadingOrder = false; }
  }

  _handleStatusFilter(status) {
    this._statusFilter = status === this._statusFilter ? null : status;
    this._page = 1;
    this._loadOrders();
  }

  _handleSearch(e) {
    this._searchTerm = e.target.value;
    clearTimeout(this._searchTimeout);
    this._searchTimeout = setTimeout(() => {
      this._page = 1;
      this._loadOrders();
    }, 300);
  }

  _handlePageChange(dir) {
    const totalPages = Math.ceil(this._totalCount / this._pageSize);
    if (dir === 'prev' && this._page > 1) { this._page--; this._loadOrders(); }
    else if (dir === 'next' && this._page < totalPages) { this._page++; this._loadOrders(); }
  }

  async _updateStatus() {
    if (!this._newStatus || this._newStatus === this._selectedOrder.status) return;
    this._saving = true;
    try {
      const res = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._selectedOrder.id}/status`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: this._newStatus, note: this._statusNote })
      });
      if (!res.ok) throw new Error('Failed to update');
      await this._loadOrders();
      await this._selectOrder({ ...this._selectedOrder, status: this._newStatus });
      this._statusNote = '';
    } catch (e) { alert('Failed to update: ' + e.message); }
    finally { this._saving = false; }
  }

  _formatCurrency(amount, code = 'USD') {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: code }).format(amount || 0);
  }

  _getStatusClass(status) {
    return status?.toLowerCase().replace(/\s+/g, '') || 'pending';
  }

  _formatDate(date) {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  _formatDateTime(date) {
    if (!date) return '-';
    return new Date(date).toLocaleString('en-US', { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  }

  render() {
    const statuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled'];
    const totalPages = Math.ceil(this._totalCount / this._pageSize);

    return html`<div class="container">
      <div class="list-panel">
        <div class="list-header">
          <h2>Orders</h2>
          <div class="search-row">
            <uui-input placeholder="Search orders..." .value=${this._searchTerm} @input=${this._handleSearch}>
              <uui-icon name="icon-search" slot="prepend"></uui-icon>
            </uui-input>
          </div>
          <div class="status-filters">
            ${statuses.map(s => html`
              <button class="status-filter ${this._statusFilter === s ? 'active' : ''}" @click=${() => this._handleStatusFilter(s)}>${s}</button>
            `)}
          </div>
        </div>
        <div class="list-content">
          ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
            this._orders.length === 0 ? html`<div class="empty-state" style="padding:40px;"><p>No orders found</p></div>` :
            this._orders.map(o => html`
              <div class="list-item ${this._selectedOrder?.id === o.id ? 'active' : ''}" @click=${() => this._selectOrder(o)}>
                <div class="order-icon ${this._getStatusClass(o.status)}">${o.status?.substring(0, 3).toUpperCase()}</div>
                <div class="order-info">
                  <div class="order-number">#${o.orderNumber}</div>
                  <div class="order-customer">${o.customerName || o.customerEmail || 'Guest'}</div>
                  <div class="order-meta">
                    <span class="status-dot ${this._getStatusClass(o.status)}"></span>
                    <span>${o.status}</span>
                    <span>${o.itemCount || 0} items</span>
                  </div>
                </div>
                <div>
                  <div class="order-total">${this._formatCurrency(o.grandTotal, o.currencyCode)}</div>
                  <div class="order-date">${this._formatDate(o.createdAt)}</div>
                </div>
              </div>
            `)}
        </div>
        ${this._totalCount > this._pageSize ? html`
          <div class="pagination">
            <span>${((this._page - 1) * this._pageSize) + 1}-${Math.min(this._page * this._pageSize, this._totalCount)} of ${this._totalCount}</span>
            <div style="display:flex;gap:8px;">
              <uui-button look="secondary" compact ?disabled=${this._page === 1} @click=${() => this._handlePageChange('prev')}>Prev</uui-button>
              <uui-button look="secondary" compact ?disabled=${this._page >= totalPages} @click=${() => this._handlePageChange('next')}>Next</uui-button>
            </div>
          </div>
        ` : ''}
      </div>
      <div class="editor-panel">
        ${this._selectedOrder ? this._renderOrderDetails() : this._renderEmptyState()}
      </div>
    </div>`;
  }

  _renderEmptyState() {
    return html`<div class="empty-state">
      <uui-icon name="icon-receipt-dollar"></uui-icon>
      <h3>Select an order to view details</h3>
      <p>Click on an order from the list to see its details</p>
    </div>`;
  }

  _renderOrderDetails() {
    const o = this._selectedOrder;
    if (this._loadingOrder) return html`<div class="loading"><uui-loader></uui-loader></div>`;

    return html`
      <div class="editor-header">
        <div class="editor-header-left">
          <h2>#${o.orderNumber}</h2>
          <span class="status-badge ${this._getStatusClass(o.status)}">${o.status}</span>
        </div>
        <div class="editor-actions">
          <uui-button look="secondary" @click=${() => window.print()}>
            <uui-icon name="icon-print"></uui-icon> Print
          </uui-button>
        </div>
      </div>
      <div class="tabs">
        <div class="tab ${this._activeTab === 'details' ? 'active' : ''}" @click=${() => this._activeTab = 'details'}>Order Details</div>
        <div class="tab ${this._activeTab === 'items' ? 'active' : ''}" @click=${() => this._activeTab = 'items'}>Items (${o.lines?.length || o.items?.length || 0})</div>
        <div class="tab ${this._activeTab === 'customer' ? 'active' : ''}" @click=${() => this._activeTab = 'customer'}>Customer</div>
        <div class="tab ${this._activeTab === 'shipping' ? 'active' : ''}" @click=${() => this._activeTab = 'shipping'}>Shipping</div>
        <div class="tab ${this._activeTab === 'status' ? 'active' : ''}" @click=${() => this._activeTab = 'status'}>Status & History</div>
      </div>
      <div class="editor-body">
        <div class="tab-content ${this._activeTab === 'details' ? 'active' : ''}">${this._renderDetailsTab()}</div>
        <div class="tab-content ${this._activeTab === 'items' ? 'active' : ''}">${this._renderItemsTab()}</div>
        <div class="tab-content ${this._activeTab === 'customer' ? 'active' : ''}">${this._renderCustomerTab()}</div>
        <div class="tab-content ${this._activeTab === 'shipping' ? 'active' : ''}">${this._renderShippingTab()}</div>
        <div class="tab-content ${this._activeTab === 'status' ? 'active' : ''}">${this._renderStatusTab()}</div>
      </div>
    `;
  }

  _renderDetailsTab() {
    const o = this._selectedOrder;
    return html`
      <div class="info-cards">
        <div class="info-card">
          <div class="info-card-label">Order Total</div>
          <div class="info-card-value large">${this._formatCurrency(o.grandTotal, o.currencyCode)}</div>
        </div>
        <div class="info-card">
          <div class="info-card-label">Items</div>
          <div class="info-card-value">${o.lines?.length || o.items?.length || o.itemCount || 0}</div>
        </div>
        <div class="info-card">
          <div class="info-card-label">Order Date</div>
          <div class="info-card-value">${this._formatDate(o.createdAt)}</div>
        </div>
        <div class="info-card">
          <div class="info-card-label">Payment</div>
          <div class="info-card-value">${o.paymentStatus || 'Pending'}</div>
        </div>
      </div>

      <div class="section">
        <div class="section-title">Quick Actions</div>
        <div class="quick-actions">
          ${o.status === 'Pending' ? html`
            <button class="quick-action primary" @click=${() => { this._newStatus = 'Confirmed'; this._updateStatus(); }}>
              <uui-icon name="icon-check"></uui-icon> Confirm Order
            </button>
          ` : ''}
          ${o.status === 'Confirmed' ? html`
            <button class="quick-action primary" @click=${() => { this._newStatus = 'Processing'; this._updateStatus(); }}>
              <uui-icon name="icon-box"></uui-icon> Start Processing
            </button>
          ` : ''}
          ${o.status === 'Processing' ? html`
            <button class="quick-action primary" @click=${() => { this._newStatus = 'Shipped'; this._updateStatus(); }}>
              <uui-icon name="icon-truck"></uui-icon> Mark as Shipped
            </button>
          ` : ''}
          ${o.status === 'Shipped' ? html`
            <button class="quick-action primary" @click=${() => { this._newStatus = 'Delivered'; this._updateStatus(); }}>
              <uui-icon name="icon-check"></uui-icon> Mark as Delivered
            </button>
          ` : ''}
          ${!['Cancelled', 'Refunded', 'Completed'].includes(o.status) ? html`
            <button class="quick-action danger" @click=${() => { this._newStatus = 'Cancelled'; this._activeTab = 'status'; }}>
              <uui-icon name="icon-delete"></uui-icon> Cancel Order
            </button>
          ` : ''}
          <button class="quick-action" @click=${() => window.open(`mailto:${o.customerEmail}`)}>
            <uui-icon name="icon-message"></uui-icon> Email Customer
          </button>
        </div>
      </div>

      <div class="section">
        <div class="section-title">Payment Information</div>
        <div class="payment-info">
          <div class="payment-icon"><uui-icon name="icon-credit-card"></uui-icon></div>
          <div class="payment-details">
            <div class="payment-method">${o.paymentMethod || 'Not specified'}</div>
            <div class="payment-status">Status: ${o.paymentStatus || 'Pending'} ${o.transactionId ? `• Transaction: ${o.transactionId}` : ''}</div>
          </div>
          <div style="text-align:right;">
            <div style="font-weight:600;">${this._formatCurrency(o.grandTotal, o.currencyCode)}</div>
            <div style="font-size:12px;color:#666;">${this._formatDateTime(o.paidAt || o.createdAt)}</div>
          </div>
        </div>
      </div>

      <div class="section">
        <div class="section-title">Order Summary</div>
        <table class="totals-table" style="width:100%;">
          <tr><td>Subtotal</td><td class="text-right">${this._formatCurrency(o.subtotal, o.currencyCode)}</td></tr>
          ${o.discountTotal ? html`<tr><td>Discount</td><td class="text-right" style="color:#28a745;">-${this._formatCurrency(o.discountTotal, o.currencyCode)}</td></tr>` : ''}
          ${o.shippingTotal ? html`<tr><td>Shipping</td><td class="text-right">${this._formatCurrency(o.shippingTotal, o.currencyCode)}</td></tr>` : ''}
          ${o.taxTotal ? html`<tr><td>Tax</td><td class="text-right">${this._formatCurrency(o.taxTotal, o.currencyCode)}</td></tr>` : ''}
          <tr class="total-row"><td>Grand Total</td><td class="text-right">${this._formatCurrency(o.grandTotal, o.currencyCode)}</td></tr>
        </table>
      </div>
    `;
  }

  _renderItemsTab() {
    const o = this._selectedOrder;
    const items = o.lines || o.items || [];
    return html`
      <div class="section">
        <div class="section-title">Order Items (${items.length})</div>
        <table class="items-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>SKU</th>
              <th class="text-right">Price</th>
              <th class="text-right">Qty</th>
              <th class="text-right">Total</th>
            </tr>
          </thead>
          <tbody>
            ${items.map(item => html`
              <tr>
                <td>
                  <div class="product-cell">
                    ${item.imageUrl ? html`<img class="product-thumb" src="${item.imageUrl}" />` : html`<div class="product-thumb"></div>`}
                    <div>
                      <div class="product-name">${item.productName}</div>
                      ${item.variantName ? html`<div class="product-sku">${item.variantName}</div>` : ''}
                    </div>
                  </div>
                </td>
                <td><span class="product-sku">${item.sku || '-'}</span></td>
                <td class="text-right">${this._formatCurrency(item.unitPrice, o.currencyCode)}</td>
                <td class="text-right">${item.quantity}</td>
                <td class="text-right"><strong>${this._formatCurrency(item.lineTotal || item.totalPrice, o.currencyCode)}</strong></td>
              </tr>
            `)}
          </tbody>
        </table>
        <table class="totals-table">
          <tr><td>Subtotal</td><td class="text-right">${this._formatCurrency(o.subtotal, o.currencyCode)}</td></tr>
          ${o.discountTotal ? html`<tr><td>Discount</td><td class="text-right" style="color:#28a745;">-${this._formatCurrency(o.discountTotal, o.currencyCode)}</td></tr>` : ''}
          ${o.shippingTotal ? html`<tr><td>Shipping</td><td class="text-right">${this._formatCurrency(o.shippingTotal, o.currencyCode)}</td></tr>` : ''}
          ${o.taxTotal ? html`<tr><td>Tax</td><td class="text-right">${this._formatCurrency(o.taxTotal, o.currencyCode)}</td></tr>` : ''}
          <tr class="total-row"><td><strong>Grand Total</strong></td><td class="text-right"><strong>${this._formatCurrency(o.grandTotal, o.currencyCode)}</strong></td></tr>
        </table>
      </div>
    `;
  }

  _renderCustomerTab() {
    const o = this._selectedOrder;
    return html`
      <div class="section">
        <div class="section-title">Customer Information</div>
        <div class="detail-grid">
          <div class="detail-item">
            <div class="detail-label">Name</div>
            <div class="detail-value">${o.customerName || 'Guest'}</div>
          </div>
          <div class="detail-item">
            <div class="detail-label">Email</div>
            <div class="detail-value"><a href="mailto:${o.customerEmail}">${o.customerEmail || '-'}</a></div>
          </div>
          <div class="detail-item">
            <div class="detail-label">Phone</div>
            <div class="detail-value">${o.customerPhone || '-'}</div>
          </div>
          <div class="detail-item">
            <div class="detail-label">Customer ID</div>
            <div class="detail-value">${o.customerId || 'Guest Checkout'}</div>
          </div>
        </div>
      </div>

      <div class="section">
        <div class="section-title">Addresses</div>
        <div class="detail-grid">
          ${o.billingAddress ? html`
            <div class="address-card">
              <h4>Billing Address</h4>
              <div class="address-line">${o.billingAddress.firstName} ${o.billingAddress.lastName}</div>
              <div class="address-line">${o.billingAddress.address1}</div>
              ${o.billingAddress.address2 ? html`<div class="address-line">${o.billingAddress.address2}</div>` : ''}
              <div class="address-line">${o.billingAddress.city}, ${o.billingAddress.stateProvince} ${o.billingAddress.postalCode}</div>
              <div class="address-line">${o.billingAddress.country}</div>
            </div>
          ` : ''}
          ${o.shippingAddress ? html`
            <div class="address-card">
              <h4>Shipping Address</h4>
              <div class="address-line">${o.shippingAddress.firstName} ${o.shippingAddress.lastName}</div>
              <div class="address-line">${o.shippingAddress.address1}</div>
              ${o.shippingAddress.address2 ? html`<div class="address-line">${o.shippingAddress.address2}</div>` : ''}
              <div class="address-line">${o.shippingAddress.city}, ${o.shippingAddress.stateProvince} ${o.shippingAddress.postalCode}</div>
              <div class="address-line">${o.shippingAddress.country}</div>
            </div>
          ` : ''}
        </div>
      </div>

      ${o.notes ? html`
        <div class="section">
          <div class="section-title">Customer Notes</div>
          <div class="address-card">${o.notes}</div>
        </div>
      ` : ''}
    `;
  }

  _renderShippingTab() {
    const o = this._selectedOrder;
    return html`
      <div class="section">
        <div class="section-title">Shipping Information</div>
        <div class="detail-grid">
          <div class="detail-item">
            <div class="detail-label">Shipping Method</div>
            <div class="detail-value">${o.shippingMethod || 'Standard Shipping'}</div>
          </div>
          <div class="detail-item">
            <div class="detail-label">Shipping Cost</div>
            <div class="detail-value">${this._formatCurrency(o.shippingTotal, o.currencyCode)}</div>
          </div>
          <div class="detail-item">
            <div class="detail-label">Tracking Number</div>
            <div class="detail-value">${o.trackingNumber || '-'}</div>
          </div>
          <div class="detail-item">
            <div class="detail-label">Carrier</div>
            <div class="detail-value">${o.shippingCarrier || '-'}</div>
          </div>
        </div>
      </div>

      ${o.shippingAddress ? html`
        <div class="section">
          <div class="section-title">Delivery Address</div>
          <div class="address-card">
            <div class="address-line"><strong>${o.shippingAddress.firstName} ${o.shippingAddress.lastName}</strong></div>
            <div class="address-line">${o.shippingAddress.address1}</div>
            ${o.shippingAddress.address2 ? html`<div class="address-line">${o.shippingAddress.address2}</div>` : ''}
            <div class="address-line">${o.shippingAddress.city}, ${o.shippingAddress.stateProvince} ${o.shippingAddress.postalCode}</div>
            <div class="address-line">${o.shippingAddress.country}</div>
            ${o.shippingAddress.phone ? html`<div class="address-line" style="margin-top:8px;">Phone: ${o.shippingAddress.phone}</div>` : ''}
          </div>
        </div>
      ` : ''}

      ${o.trackingNumber ? html`
        <div class="section">
          <div class="section-title">Tracking</div>
          <div class="quick-actions">
            <button class="quick-action primary" @click=${() => window.open(`https://track.aftership.com/${o.trackingNumber}`, '_blank')}>
              <uui-icon name="icon-truck"></uui-icon> Track Package
            </button>
          </div>
        </div>
      ` : ''}
    `;
  }

  _renderStatusTab() {
    const o = this._selectedOrder;
    const statuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled', 'Refunded', 'OnHold', 'Failed'];
    const history = o.statusHistory || [{ status: o.status, date: o.createdAt, note: 'Order created' }];

    return html`
      <div class="section">
        <div class="section-title">Update Status</div>
        <div class="form-row">
          <div class="form-group">
            <label>New Status</label>
            <select .value=${this._newStatus} @change=${e => this._newStatus = e.target.value}>
              ${statuses.map(s => html`<option value="${s}" ?selected=${this._newStatus === s}>${s}</option>`)}
            </select>
          </div>
        </div>
        <div class="form-group">
          <label>Status Note (optional)</label>
          <textarea .value=${this._statusNote} @input=${e => this._statusNote = e.target.value} placeholder="Add a note about this status change..."></textarea>
        </div>
        <uui-button look="primary" @click=${this._updateStatus} ?disabled=${this._saving || this._newStatus === o.status}>
          ${this._saving ? 'Updating...' : 'Update Status'}
        </uui-button>
      </div>

      <div class="section">
        <div class="section-title">Status History</div>
        <div class="timeline">
          ${history.map((h, i) => html`
            <div class="timeline-item">
              <div class="timeline-dot ${h.status === 'Completed' || h.status === 'Delivered' ? 'completed' : h.status === 'Cancelled' ? 'cancelled' : ''}"></div>
              <div class="timeline-content">
                <div class="timeline-title">${h.status}</div>
                <div class="timeline-date">${this._formatDateTime(h.date || h.createdAt)}</div>
                ${h.note ? html`<div class="timeline-note">${h.note}</div>` : ''}
              </div>
            </div>
          `)}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-collection', OrderCollection);
export default OrderCollection;
