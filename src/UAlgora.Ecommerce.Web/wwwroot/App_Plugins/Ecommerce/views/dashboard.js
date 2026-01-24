import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * E-commerce Dashboard Element
 * Comprehensive analytics dashboard with Umbraco Commerce-style widgets.
 */
export class EcommerceDashboard extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: 24px;
      background: #f5f5f5;
      min-height: 100vh;
    }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .dashboard-header h1 {
      margin: 0;
      font-size: 28px;
      font-weight: 600;
      color: #1b264f;
    }

    .header-actions {
      display: flex;
      gap: 12px;
      align-items: center;
    }

    .period-selector {
      display: flex;
      gap: 4px;
      background: #fff;
      padding: 4px;
      border-radius: 8px;
      border: 1px solid #e0e0e0;
    }

    .period-btn {
      padding: 8px 16px;
      border: none;
      background: transparent;
      border-radius: 6px;
      cursor: pointer;
      font-size: 13px;
      font-weight: 500;
      color: #666;
      transition: all 0.2s;
    }

    .period-btn:hover {
      background: #f0f0f0;
    }

    .period-btn.active {
      background: #1b264f;
      color: #fff;
    }

    /* Revenue Cards Row */
    .revenue-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .revenue-card {
      background: #fff;
      border-radius: 12px;
      padding: 20px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
      position: relative;
      overflow: hidden;
    }

    .revenue-card::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 4px;
    }

    .revenue-card.today::before { background: linear-gradient(90deg, #667eea, #764ba2); }
    .revenue-card.week::before { background: linear-gradient(90deg, #11998e, #38ef7d); }
    .revenue-card.month::before { background: linear-gradient(90deg, #f093fb, #f5576c); }
    .revenue-card.year::before { background: linear-gradient(90deg, #4facfe, #00f2fe); }

    .revenue-label {
      font-size: 12px;
      color: #888;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 8px;
    }

    .revenue-value {
      font-size: 28px;
      font-weight: 700;
      color: #1b264f;
      margin-bottom: 8px;
    }

    .revenue-change {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;
    }

    .revenue-change.positive { color: #22c55e; }
    .revenue-change.negative { color: #ef4444; }

    .revenue-orders {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    /* Main Grid */
    .main-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 24px;
      margin-bottom: 24px;
    }

    /* Widget Cards */
    .widget {
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
      overflow: hidden;
    }

    .widget-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      border-bottom: 1px solid #f0f0f0;
    }

    .widget-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #1b264f;
    }

    .widget-body {
      padding: 20px;
    }

    /* Order Funnel */
    .funnel-stages {
      display: flex;
      justify-content: space-between;
      position: relative;
      padding: 20px 0;
    }

    .funnel-stage {
      flex: 1;
      text-align: center;
      position: relative;
    }

    .funnel-stage::after {
      content: '';
      position: absolute;
      top: 20px;
      right: 0;
      width: 50%;
      height: 2px;
      background: #e0e0e0;
    }

    .funnel-stage:last-child::after {
      display: none;
    }

    .funnel-stage::before {
      content: '';
      position: absolute;
      top: 20px;
      left: 0;
      width: 50%;
      height: 2px;
      background: #e0e0e0;
    }

    .funnel-stage:first-child::before {
      display: none;
    }

    .funnel-icon {
      width: 44px;
      height: 44px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 12px;
      font-size: 18px;
      position: relative;
      z-index: 1;
    }

    .funnel-stage.cart .funnel-icon { background: #fef3c7; color: #d97706; }
    .funnel-stage.checkout .funnel-icon { background: #dbeafe; color: #2563eb; }
    .funnel-stage.paid .funnel-icon { background: #d1fae5; color: #059669; }
    .funnel-stage.shipped .funnel-icon { background: #ede9fe; color: #7c3aed; }

    .funnel-value {
      font-size: 24px;
      font-weight: 700;
      color: #1b264f;
      margin-bottom: 4px;
    }

    .funnel-label {
      font-size: 12px;
      color: #888;
    }

    .funnel-rate {
      font-size: 11px;
      color: #22c55e;
      margin-top: 4px;
    }

    /* Top Products */
    .product-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .product-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      background: #f9f9f9;
      border-radius: 8px;
      transition: background 0.2s;
    }

    .product-item:hover {
      background: #f0f0f0;
    }

    .product-rank {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: #1b264f;
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: 600;
    }

    .product-rank.gold { background: linear-gradient(135deg, #f5af19, #f12711); }
    .product-rank.silver { background: linear-gradient(135deg, #bdc3c7, #2c3e50); }
    .product-rank.bronze { background: linear-gradient(135deg, #c9820a, #8B4513); }

    .product-image {
      width: 44px;
      height: 44px;
      border-radius: 8px;
      background: #e0e0e0;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #999;
    }

    .product-info {
      flex: 1;
      min-width: 0;
    }

    .product-name {
      font-weight: 500;
      color: #1b264f;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .product-sku {
      font-size: 12px;
      color: #888;
    }

    .product-stats {
      text-align: right;
    }

    .product-revenue {
      font-weight: 600;
      color: #22c55e;
    }

    .product-qty {
      font-size: 12px;
      color: #888;
    }

    /* Low Stock Alerts */
    .alert-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
      max-height: 280px;
      overflow-y: auto;
    }

    .alert-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      background: #fef2f2;
      border-radius: 8px;
      border-left: 3px solid #ef4444;
    }

    .alert-item.warning {
      background: #fffbeb;
      border-left-color: #f59e0b;
    }

    .alert-icon {
      font-size: 18px;
    }

    .alert-item .alert-icon { color: #ef4444; }
    .alert-item.warning .alert-icon { color: #f59e0b; }

    .alert-info {
      flex: 1;
    }

    .alert-name {
      font-weight: 500;
      color: #1b264f;
      font-size: 14px;
    }

    .alert-stock {
      font-size: 12px;
      color: #888;
    }

    .alert-action {
      font-size: 12px;
      color: #2563eb;
      cursor: pointer;
    }

    /* Refund Rate Widget */
    .refund-stats {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .refund-stat {
      text-align: center;
      padding: 16px;
      background: #f9f9f9;
      border-radius: 8px;
    }

    .refund-value {
      font-size: 32px;
      font-weight: 700;
    }

    .refund-stat.rate .refund-value { color: #f59e0b; }
    .refund-stat.amount .refund-value { color: #ef4444; }

    .refund-label {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    .refund-trend {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      margin-top: 8px;
      font-size: 12px;
    }

    .refund-trend.up { color: #ef4444; }
    .refund-trend.down { color: #22c55e; }

    /* Top Buyers */
    .buyer-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .buyer-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      background: #f9f9f9;
      border-radius: 8px;
    }

    .buyer-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 14px;
    }

    .buyer-info {
      flex: 1;
    }

    .buyer-name {
      font-weight: 500;
      color: #1b264f;
    }

    .buyer-email {
      font-size: 12px;
      color: #888;
    }

    .buyer-stats {
      text-align: right;
    }

    .buyer-total {
      font-weight: 600;
      color: #22c55e;
    }

    .buyer-orders {
      font-size: 12px;
      color: #888;
    }

    /* Recent Activity */
    .activity-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      max-height: 300px;
      overflow-y: auto;
    }

    .activity-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px;
      background: #f9f9f9;
      border-radius: 8px;
    }

    .activity-icon {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 14px;
    }

    .activity-icon.order { background: #dbeafe; color: #2563eb; }
    .activity-icon.payment { background: #d1fae5; color: #059669; }
    .activity-icon.refund { background: #fef2f2; color: #ef4444; }
    .activity-icon.customer { background: #ede9fe; color: #7c3aed; }

    .activity-content {
      flex: 1;
    }

    .activity-text {
      font-size: 14px;
      color: #1b264f;
    }

    .activity-text strong {
      font-weight: 600;
    }

    .activity-time {
      font-size: 12px;
      color: #888;
      margin-top: 2px;
    }

    .activity-amount {
      font-weight: 600;
      color: #22c55e;
      font-size: 14px;
    }

    /* Quick Actions Grid */
    .quick-actions {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .quick-action {
      background: #fff;
      border-radius: 12px;
      padding: 20px;
      text-align: center;
      cursor: pointer;
      transition: all 0.2s;
      border: 1px solid transparent;
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
    }

    .quick-action:hover {
      border-color: #667eea;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.15);
    }

    .quick-action-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 12px;
      font-size: 22px;
    }

    .quick-action.products .quick-action-icon { background: #dbeafe; color: #2563eb; }
    .quick-action.orders .quick-action-icon { background: #d1fae5; color: #059669; }
    .quick-action.customers .quick-action-icon { background: #ede9fe; color: #7c3aed; }
    .quick-action.discounts .quick-action-icon { background: #fef3c7; color: #d97706; }

    .quick-action-label {
      font-weight: 500;
      color: #1b264f;
    }

    .quick-action-count {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    /* Secondary Grid */
    .secondary-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 24px;
    }

    /* Loading State */
    .loading {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 60px;
    }

    /* Empty State */
    .empty-state {
      text-align: center;
      padding: 40px;
      color: #888;
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: 16px;
      opacity: 0.5;
    }

    /* Responsive */
    @media (max-width: 1400px) {
      .revenue-grid { grid-template-columns: repeat(2, 1fr); }
      .main-grid { grid-template-columns: 1fr; }
      .secondary-grid { grid-template-columns: repeat(2, 1fr); }
      .quick-actions { grid-template-columns: repeat(2, 1fr); }
    }

    @media (max-width: 900px) {
      .revenue-grid { grid-template-columns: 1fr; }
      .secondary-grid { grid-template-columns: 1fr; }
      .quick-actions { grid-template-columns: 1fr; }
    }
  `;

  static properties = {
    _loading: { type: Boolean, state: true },
    _overview: { type: Object, state: true },
    _topProducts: { type: Array, state: true },
    _lowStock: { type: Array, state: true },
    _topBuyers: { type: Array, state: true },
    _recentActivity: { type: Array, state: true },
    _refundStats: { type: Object, state: true },
    _selectedPeriod: { type: String, state: true },
    _error: { type: String, state: true }
  };

  constructor() {
    super();
    this._loading = true;
    this._overview = null;
    this._topProducts = [];
    this._lowStock = [];
    this._topBuyers = [];
    this._recentActivity = [];
    this._refundStats = null;
    this._selectedPeriod = 'month';
    this._error = null;
  }

  async connectedCallback() {
    super.connectedCallback();
    await this._loadAllData();
  }

  async _loadAllData() {
    this._loading = true;
    try {
      await Promise.all([
        this._loadDashboardData(),
        this._loadTopProducts(),
        this._loadLowStock(),
        this._loadTopBuyers(),
        this._loadRecentActivity(),
        this._loadRefundStats()
      ]);
      this._error = null;
    } catch (error) {
      this._error = error.message;
      console.error('Error loading dashboard:', error);
    } finally {
      this._loading = false;
    }
  }

  async _loadDashboardData() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/overview', {
        headers: { 'Accept': 'application/json' },
        credentials: 'include'
      });
      if (response.ok) {
        this._overview = await response.json();
      }
    } catch (error) {
      console.error('Error loading overview:', error);
      // Use mock data for demo
      this._overview = {
        todayRevenue: 2847.50,
        todayOrders: 12,
        todayChange: 15.3,
        weekRevenue: 18432.00,
        weekOrders: 87,
        weekChange: 8.2,
        monthRevenue: 67543.25,
        monthOrders: 342,
        monthChange: 12.5,
        yearRevenue: 524890.00,
        yearOrders: 4231,
        yearChange: 23.7,
        cartCount: 45,
        checkoutCount: 28,
        paidCount: 24,
        shippedCount: 21,
        totalProducts: 156,
        totalOrders: 4231,
        totalCustomers: 1847,
        activeDiscounts: 8,
        currencyCode: 'USD'
      };
    }
  }

  async _loadTopProducts() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/top-products?limit=5', {
        headers: { 'Accept': 'application/json' },
        credentials: 'include'
      });
      if (response.ok) {
        this._topProducts = await response.json();
      }
    } catch (error) {
      console.error('Error loading top products:', error);
      this._topProducts = [
        { name: 'Premium Wireless Headphones', sku: 'WH-1000XM5', revenue: 12450.00, quantity: 83 },
        { name: 'Smart Watch Pro', sku: 'SW-PRO-2024', revenue: 9870.00, quantity: 47 },
        { name: 'Bluetooth Speaker', sku: 'BS-MEGA-100', revenue: 7654.00, quantity: 127 },
        { name: 'USB-C Hub 7-in-1', sku: 'HUB-7IN1', revenue: 5432.00, quantity: 234 },
        { name: 'Mechanical Keyboard', sku: 'KB-MECH-RGB', revenue: 4890.00, quantity: 56 }
      ];
    }
  }

  async _loadLowStock() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/low-stock?threshold=10', {
        headers: { 'Accept': 'application/json' },
        credentials: 'include'
      });
      if (response.ok) {
        this._lowStock = await response.json();
      }
    } catch (error) {
      console.error('Error loading low stock:', error);
      this._lowStock = [
        { name: 'Premium Wireless Headphones', sku: 'WH-1000XM5', stock: 3, threshold: 10 },
        { name: 'Smart Watch Pro', sku: 'SW-PRO-2024', stock: 5, threshold: 10 },
        { name: 'Leather Wallet', sku: 'LW-PREM-BLK', stock: 8, threshold: 15 }
      ];
    }
  }

  async _loadTopBuyers() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/top-buyers?limit=5', {
        headers: { 'Accept': 'application/json' },
        credentials: 'include'
      });
      if (response.ok) {
        this._topBuyers = await response.json();
      }
    } catch (error) {
      console.error('Error loading top buyers:', error);
      this._topBuyers = [
        { name: 'John Smith', email: 'john.smith@email.com', total: 4523.00, orders: 12 },
        { name: 'Sarah Johnson', email: 'sarah.j@company.com', total: 3890.00, orders: 8 },
        { name: 'Mike Chen', email: 'mike.chen@gmail.com', total: 2765.00, orders: 15 },
        { name: 'Emily Brown', email: 'emily.b@email.com', total: 2340.00, orders: 6 },
        { name: 'David Wilson', email: 'dwilson@corp.com', total: 1987.00, orders: 9 }
      ];
    }
  }

  async _loadRecentActivity() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/recent-activity?limit=10', {
        headers: { 'Accept': 'application/json' },
        credentials: 'include'
      });
      if (response.ok) {
        this._recentActivity = await response.json();
      }
    } catch (error) {
      console.error('Error loading recent activity:', error);
      this._recentActivity = [
        { type: 'order', text: 'New order <strong>#1234</strong> from John Smith', amount: 189.99, time: '2 mins ago' },
        { type: 'payment', text: 'Payment received for order <strong>#1231</strong>', amount: 245.00, time: '15 mins ago' },
        { type: 'customer', text: 'New customer <strong>Sarah Johnson</strong> registered', time: '32 mins ago' },
        { type: 'refund', text: 'Refund processed for order <strong>#1198</strong>', amount: 89.99, time: '1 hour ago' },
        { type: 'order', text: 'Order <strong>#1230</strong> shipped via FedEx', time: '2 hours ago' }
      ];
    }
  }

  async _loadRefundStats() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/refund-stats', {
        headers: { 'Accept': 'application/json' },
        credentials: 'include'
      });
      if (response.ok) {
        this._refundStats = await response.json();
      }
    } catch (error) {
      console.error('Error loading refund stats:', error);
      this._refundStats = {
        rate: 2.4,
        rateChange: -0.3,
        totalRefunds: 1234.56,
        refundCount: 8
      };
    }
  }

  _handlePeriodChange(period) {
    this._selectedPeriod = period;
    this._loadAllData();
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    if (amount == null) return '$0.00';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode
    }).format(amount);
  }

  _getInitials(name) {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  render() {
    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    if (this._error) {
      return html`
        <uui-box>
          <div slot="headline">Error</div>
          <p>${this._error}</p>
          <uui-button @click=${this._loadAllData}>Retry</uui-button>
        </uui-box>
      `;
    }

    return html`
      <!-- Header -->
      <div class="dashboard-header">
        <h1>Commerce Dashboard</h1>
        <div class="header-actions">
          <div class="period-selector">
            <button class="period-btn ${this._selectedPeriod === 'today' ? 'active' : ''}" @click=${() => this._handlePeriodChange('today')}>Today</button>
            <button class="period-btn ${this._selectedPeriod === 'week' ? 'active' : ''}" @click=${() => this._handlePeriodChange('week')}>Week</button>
            <button class="period-btn ${this._selectedPeriod === 'month' ? 'active' : ''}" @click=${() => this._handlePeriodChange('month')}>Month</button>
            <button class="period-btn ${this._selectedPeriod === 'year' ? 'active' : ''}" @click=${() => this._handlePeriodChange('year')}>Year</button>
          </div>
          <uui-button look="primary" @click=${this._loadAllData}>
            <uui-icon name="icon-refresh"></uui-icon> Refresh
          </uui-button>
        </div>
      </div>

      <!-- Revenue Cards -->
      <div class="revenue-grid">
        <div class="revenue-card today">
          <div class="revenue-label">Today's Revenue</div>
          <div class="revenue-value">${this._formatCurrency(this._overview?.todayRevenue)}</div>
          <div class="revenue-change ${(this._overview?.todayChange || 0) >= 0 ? 'positive' : 'negative'}">
            <uui-icon name="${(this._overview?.todayChange || 0) >= 0 ? 'icon-arrow-up' : 'icon-arrow-down'}"></uui-icon>
            ${Math.abs(this._overview?.todayChange || 0)}% vs yesterday
          </div>
          <div class="revenue-orders">${this._overview?.todayOrders || 0} orders</div>
        </div>
        <div class="revenue-card week">
          <div class="revenue-label">This Week</div>
          <div class="revenue-value">${this._formatCurrency(this._overview?.weekRevenue)}</div>
          <div class="revenue-change ${(this._overview?.weekChange || 0) >= 0 ? 'positive' : 'negative'}">
            <uui-icon name="${(this._overview?.weekChange || 0) >= 0 ? 'icon-arrow-up' : 'icon-arrow-down'}"></uui-icon>
            ${Math.abs(this._overview?.weekChange || 0)}% vs last week
          </div>
          <div class="revenue-orders">${this._overview?.weekOrders || 0} orders</div>
        </div>
        <div class="revenue-card month">
          <div class="revenue-label">This Month</div>
          <div class="revenue-value">${this._formatCurrency(this._overview?.monthRevenue)}</div>
          <div class="revenue-change ${(this._overview?.monthChange || 0) >= 0 ? 'positive' : 'negative'}">
            <uui-icon name="${(this._overview?.monthChange || 0) >= 0 ? 'icon-arrow-up' : 'icon-arrow-down'}"></uui-icon>
            ${Math.abs(this._overview?.monthChange || 0)}% vs last month
          </div>
          <div class="revenue-orders">${this._overview?.monthOrders || 0} orders</div>
        </div>
        <div class="revenue-card year">
          <div class="revenue-label">Year to Date</div>
          <div class="revenue-value">${this._formatCurrency(this._overview?.yearRevenue)}</div>
          <div class="revenue-change ${(this._overview?.yearChange || 0) >= 0 ? 'positive' : 'negative'}">
            <uui-icon name="${(this._overview?.yearChange || 0) >= 0 ? 'icon-arrow-up' : 'icon-arrow-down'}"></uui-icon>
            ${Math.abs(this._overview?.yearChange || 0)}% vs last year
          </div>
          <div class="revenue-orders">${this._overview?.yearOrders || 0} orders</div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="quick-actions">
        <div class="quick-action products">
          <div class="quick-action-icon"><uui-icon name="icon-box"></uui-icon></div>
          <div class="quick-action-label">Products</div>
          <div class="quick-action-count">${this._overview?.totalProducts || 0} items</div>
        </div>
        <div class="quick-action orders">
          <div class="quick-action-icon"><uui-icon name="icon-receipt-dollar"></uui-icon></div>
          <div class="quick-action-label">Orders</div>
          <div class="quick-action-count">${this._overview?.totalOrders || 0} total</div>
        </div>
        <div class="quick-action customers">
          <div class="quick-action-icon"><uui-icon name="icon-users"></uui-icon></div>
          <div class="quick-action-label">Customers</div>
          <div class="quick-action-count">${this._overview?.totalCustomers || 0} registered</div>
        </div>
        <div class="quick-action discounts">
          <div class="quick-action-icon"><uui-icon name="icon-sale"></uui-icon></div>
          <div class="quick-action-label">Discounts</div>
          <div class="quick-action-count">${this._overview?.activeDiscounts || 0} active</div>
        </div>
      </div>

      <!-- Main Grid -->
      <div class="main-grid">
        <!-- Order Funnel -->
        <div class="widget">
          <div class="widget-header">
            <h3>Order Funnel</h3>
            <uui-button look="secondary" compact>View Details</uui-button>
          </div>
          <div class="widget-body">
            <div class="funnel-stages">
              <div class="funnel-stage cart">
                <div class="funnel-icon"><uui-icon name="icon-shopping-basket"></uui-icon></div>
                <div class="funnel-value">${this._overview?.cartCount || 0}</div>
                <div class="funnel-label">In Cart</div>
              </div>
              <div class="funnel-stage checkout">
                <div class="funnel-icon"><uui-icon name="icon-credit-card"></uui-icon></div>
                <div class="funnel-value">${this._overview?.checkoutCount || 0}</div>
                <div class="funnel-label">Checkout</div>
                <div class="funnel-rate">${this._overview?.cartCount ? Math.round((this._overview?.checkoutCount / this._overview?.cartCount) * 100) : 0}% conversion</div>
              </div>
              <div class="funnel-stage paid">
                <div class="funnel-icon"><uui-icon name="icon-check"></uui-icon></div>
                <div class="funnel-value">${this._overview?.paidCount || 0}</div>
                <div class="funnel-label">Paid</div>
                <div class="funnel-rate">${this._overview?.checkoutCount ? Math.round((this._overview?.paidCount / this._overview?.checkoutCount) * 100) : 0}% success</div>
              </div>
              <div class="funnel-stage shipped">
                <div class="funnel-icon"><uui-icon name="icon-truck"></uui-icon></div>
                <div class="funnel-value">${this._overview?.shippedCount || 0}</div>
                <div class="funnel-label">Shipped</div>
                <div class="funnel-rate">${this._overview?.paidCount ? Math.round((this._overview?.shippedCount / this._overview?.paidCount) * 100) : 0}% fulfilled</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Low Stock Alerts -->
        <div class="widget">
          <div class="widget-header">
            <h3>Low Stock Alerts</h3>
            <span style="background:#fef2f2;color:#ef4444;padding:4px 8px;border-radius:12px;font-size:12px;font-weight:600;">${this._lowStock.length} items</span>
          </div>
          <div class="widget-body">
            ${this._lowStock.length === 0 ? html`
              <div class="empty-state">
                <uui-icon name="icon-check"></uui-icon>
                <p>All products are well stocked!</p>
              </div>
            ` : html`
              <div class="alert-list">
                ${this._lowStock.map(item => html`
                  <div class="alert-item ${item.stock <= 5 ? '' : 'warning'}">
                    <uui-icon class="alert-icon" name="icon-alert"></uui-icon>
                    <div class="alert-info">
                      <div class="alert-name">${item.name}</div>
                      <div class="alert-stock">${item.stock} left (threshold: ${item.threshold})</div>
                    </div>
                    <span class="alert-action">Restock</span>
                  </div>
                `)}
              </div>
            `}
          </div>
        </div>
      </div>

      <!-- Secondary Grid -->
      <div class="secondary-grid">
        <!-- Top Products -->
        <div class="widget">
          <div class="widget-header">
            <h3>Top Products</h3>
            <uui-button look="secondary" compact>View All</uui-button>
          </div>
          <div class="widget-body">
            <div class="product-list">
              ${this._topProducts.map((product, index) => html`
                <div class="product-item">
                  <div class="product-rank ${index === 0 ? 'gold' : index === 1 ? 'silver' : index === 2 ? 'bronze' : ''}">${index + 1}</div>
                  <div class="product-image"><uui-icon name="icon-box"></uui-icon></div>
                  <div class="product-info">
                    <div class="product-name">${product.name}</div>
                    <div class="product-sku">${product.sku}</div>
                  </div>
                  <div class="product-stats">
                    <div class="product-revenue">${this._formatCurrency(product.revenue)}</div>
                    <div class="product-qty">${product.quantity} sold</div>
                  </div>
                </div>
              `)}
            </div>
          </div>
        </div>

        <!-- Top Buyers -->
        <div class="widget">
          <div class="widget-header">
            <h3>Top Buyers</h3>
            <uui-button look="secondary" compact>View All</uui-button>
          </div>
          <div class="widget-body">
            <div class="buyer-list">
              ${this._topBuyers.map(buyer => html`
                <div class="buyer-item">
                  <div class="buyer-avatar">${this._getInitials(buyer.name)}</div>
                  <div class="buyer-info">
                    <div class="buyer-name">${buyer.name}</div>
                    <div class="buyer-email">${buyer.email}</div>
                  </div>
                  <div class="buyer-stats">
                    <div class="buyer-total">${this._formatCurrency(buyer.total)}</div>
                    <div class="buyer-orders">${buyer.orders} orders</div>
                  </div>
                </div>
              `)}
            </div>
          </div>
        </div>

        <!-- Refund Rate -->
        <div class="widget">
          <div class="widget-header">
            <h3>Refund Analytics</h3>
            <uui-button look="secondary" compact>View Details</uui-button>
          </div>
          <div class="widget-body">
            <div class="refund-stats">
              <div class="refund-stat rate">
                <div class="refund-value">${this._refundStats?.rate || 0}%</div>
                <div class="refund-label">Refund Rate</div>
                <div class="refund-trend ${(this._refundStats?.rateChange || 0) < 0 ? 'down' : 'up'}">
                  <uui-icon name="${(this._refundStats?.rateChange || 0) < 0 ? 'icon-arrow-down' : 'icon-arrow-up'}"></uui-icon>
                  ${Math.abs(this._refundStats?.rateChange || 0)}% vs last period
                </div>
              </div>
              <div class="refund-stat amount">
                <div class="refund-value">${this._formatCurrency(this._refundStats?.totalRefunds)}</div>
                <div class="refund-label">Total Refunded</div>
                <div class="refund-trend">${this._refundStats?.refundCount || 0} refunds</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="widget" style="margin-top: 24px;">
        <div class="widget-header">
          <h3>Recent Activity</h3>
          <uui-button look="secondary" compact>View All</uui-button>
        </div>
        <div class="widget-body">
          <div class="activity-list">
            ${this._recentActivity.map(activity => html`
              <div class="activity-item">
                <div class="activity-icon ${activity.type}">
                  <uui-icon name="${activity.type === 'order' ? 'icon-receipt-dollar' : activity.type === 'payment' ? 'icon-check' : activity.type === 'refund' ? 'icon-undo' : 'icon-user'}"></uui-icon>
                </div>
                <div class="activity-content">
                  <div class="activity-text">${this._unsafeHTML(activity.text)}</div>
                  <div class="activity-time">${activity.time}</div>
                </div>
                ${activity.amount ? html`<div class="activity-amount">${this._formatCurrency(activity.amount)}</div>` : ''}
              </div>
            `)}
          </div>
        </div>
      </div>
    `;
  }

  _unsafeHTML(htmlString) {
    const template = document.createElement('template');
    template.innerHTML = htmlString;
    return template.content.cloneNode(true);
  }
}

customElements.define('ecommerce-dashboard', EcommerceDashboard);

export default EcommerceDashboard;
