import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";

/**
 * Return Collection with Inline Editor
 * Umbraco Commerce-style returns and refunds management.
 */
export class ReturnCollection extends UmbElementMixin(LitElement) {
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
      display: flex;
      height: 100%;
      background: #f5f5f5;
    }

    /* List Panel */
    .list-panel {
      width: 400px;
      min-width: 400px;
      background: #fff;
      border-right: 1px solid #e0e0e0;
      display: flex;
      flex-direction: column;
      height: 100%;
    }

    .list-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
    }

    .list-header h2 {
      margin: 0 0 16px 0;
      font-size: 20px;
      font-weight: 600;
      color: #1b264f;
    }

    .search-box {
      display: flex;
      align-items: center;
      background: #f5f5f5;
      border-radius: 8px;
      padding: 8px 12px;
    }

    .search-box input {
      flex: 1;
      border: none;
      background: transparent;
      outline: none;
      font-size: 14px;
    }

    /* Status Filter Chips */
    .status-filters {
      display: flex;
      gap: 6px;
      padding: 12px 20px;
      border-bottom: 1px solid #e0e0e0;
      flex-wrap: wrap;
    }

    .status-chip {
      padding: 6px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 500;
      cursor: pointer;
      border: 1px solid #e0e0e0;
      background: #fff;
      color: #666;
      transition: all 0.2s;
    }

    .status-chip:hover { background: #f5f5f5; }
    .status-chip.active { border-color: transparent; }
    .status-chip.all.active { background: #1b264f; color: #fff; }
    .status-chip.pending.active { background: #f59e0b; color: #fff; }
    .status-chip.approved.active { background: #22c55e; color: #fff; }
    .status-chip.rejected.active { background: #ef4444; color: #fff; }
    .status-chip.received.active { background: #3b82f6; color: #fff; }
    .status-chip.refunded.active { background: #8b5cf6; color: #fff; }

    /* Return List */
    .return-list {
      flex: 1;
      overflow-y: auto;
      padding: 12px;
    }

    .return-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px;
      margin-bottom: 8px;
      background: #fff;
      border: 1px solid #e0e0e0;
      border-radius: 10px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .return-item:hover {
      border-color: #667eea;
      box-shadow: 0 2px 8px rgba(102, 126, 234, 0.15);
    }

    .return-item.selected {
      border-color: #667eea;
      background: #f8f9ff;
    }

    .return-icon {
      width: 44px;
      height: 44px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 18px;
    }

    .return-icon.pending { background: linear-gradient(135deg, #f59e0b, #d97706); color: #fff; }
    .return-icon.approved { background: linear-gradient(135deg, #22c55e, #16a34a); color: #fff; }
    .return-icon.rejected { background: linear-gradient(135deg, #ef4444, #dc2626); color: #fff; }
    .return-icon.received { background: linear-gradient(135deg, #3b82f6, #2563eb); color: #fff; }
    .return-icon.refunded { background: linear-gradient(135deg, #8b5cf6, #7c3aed); color: #fff; }

    .return-info {
      flex: 1;
      min-width: 0;
    }

    .return-number {
      font-weight: 600;
      color: #1b264f;
      font-size: 14px;
    }

    .return-meta {
      font-size: 12px;
      color: #888;
      margin-top: 4px;
    }

    .return-amount {
      text-align: right;
    }

    .amount-value {
      font-weight: 600;
      font-size: 16px;
      color: #ef4444;
    }

    .amount-label {
      font-size: 11px;
      color: #888;
    }

    /* Editor Panel */
    .editor-panel {
      flex: 1;
      display: flex;
      flex-direction: column;
      height: 100%;
      overflow: hidden;
    }

    .editor-header {
      padding: 20px 24px;
      background: #fff;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .editor-header h2 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #1b264f;
    }

    .editor-actions {
      display: flex;
      gap: 8px;
    }

    /* Tabs */
    .tabs {
      display: flex;
      background: #fff;
      border-bottom: 1px solid #e0e0e0;
      padding: 0 24px;
    }

    .tab {
      padding: 14px 20px;
      font-size: 14px;
      font-weight: 500;
      color: #666;
      cursor: pointer;
      border-bottom: 2px solid transparent;
      transition: all 0.2s;
    }

    .tab:hover { color: #1b264f; }
    .tab.active { color: #667eea; border-bottom-color: #667eea; }

    /* Tab Content */
    .tab-content {
      flex: 1;
      overflow-y: auto;
      padding: 24px;
    }

    /* Status Badge */
    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 12px;
      border-radius: 20px;
      font-size: 13px;
      font-weight: 500;
    }

    .status-badge.pending { background: #fef3c7; color: #b45309; }
    .status-badge.approved { background: #d1fae5; color: #059669; }
    .status-badge.rejected { background: #fee2e2; color: #dc2626; }
    .status-badge.received { background: #dbeafe; color: #2563eb; }
    .status-badge.refunded { background: #ede9fe; color: #7c3aed; }

    /* Info Cards */
    .info-cards {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .info-card {
      background: #fff;
      border-radius: 12px;
      padding: 16px;
      border: 1px solid #e0e0e0;
    }

    .info-card-label {
      font-size: 12px;
      color: #888;
      margin-bottom: 8px;
    }

    .info-card-value {
      font-size: 24px;
      font-weight: 700;
      color: #1b264f;
    }

    .info-card-value.requested { color: #f59e0b; }
    .info-card-value.approved { color: #22c55e; }
    .info-card-value.refunded { color: #8b5cf6; }

    /* Form Section */
    .form-section {
      background: #fff;
      border-radius: 12px;
      padding: 20px;
      margin-bottom: 20px;
      border: 1px solid #e0e0e0;
    }

    .form-section-title {
      font-size: 14px;
      font-weight: 600;
      color: #1b264f;
      margin-bottom: 16px;
      padding-bottom: 12px;
      border-bottom: 1px solid #f0f0f0;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 500;
      color: #555;
      margin-bottom: 6px;
    }

    .form-group input,
    .form-group select,
    .form-group textarea {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #ddd;
      border-radius: 8px;
      font-size: 14px;
      box-sizing: border-box;
    }

    .form-group input[readonly] {
      background: #f5f5f5;
      cursor: not-allowed;
    }

    /* Return Items Table */
    .items-table {
      width: 100%;
      border-collapse: collapse;
    }

    .items-table th,
    .items-table td {
      padding: 12px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    .items-table th {
      background: #f9f9f9;
      font-weight: 600;
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
    }

    .items-table tr:hover td {
      background: #f9f9f9;
    }

    .item-product {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .item-image {
      width: 40px;
      height: 40px;
      border-radius: 6px;
      background: #e0e0e0;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .item-name {
      font-weight: 500;
      color: #1b264f;
    }

    .item-sku {
      font-size: 12px;
      color: #888;
    }

    .condition-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 11px;
      font-weight: 500;
    }

    .condition-badge.unopened { background: #d1fae5; color: #059669; }
    .condition-badge.opened { background: #fef3c7; color: #b45309; }
    .condition-badge.defective { background: #fee2e2; color: #dc2626; }
    .condition-badge.damaged { background: #fee2e2; color: #dc2626; }

    /* Quick Actions */
    .quick-actions {
      display: flex;
      gap: 12px;
      margin-bottom: 24px;
    }

    .action-btn {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 20px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      border: none;
      transition: all 0.2s;
    }

    .action-btn.primary {
      background: #22c55e;
      color: #fff;
    }

    .action-btn.primary:hover {
      background: #16a34a;
    }

    .action-btn.danger {
      background: #fee2e2;
      color: #dc2626;
      border: 1px solid #fecaca;
    }

    .action-btn.danger:hover {
      background: #fecaca;
    }

    .action-btn.secondary {
      background: #f5f5f5;
      color: #555;
      border: 1px solid #e0e0e0;
    }

    .action-btn.secondary:hover {
      background: #e0e0e0;
    }

    .action-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    /* Timeline */
    .timeline {
      position: relative;
      padding-left: 30px;
    }

    .timeline::before {
      content: '';
      position: absolute;
      left: 10px;
      top: 0;
      bottom: 0;
      width: 2px;
      background: #e0e0e0;
    }

    .timeline-item {
      position: relative;
      padding-bottom: 24px;
    }

    .timeline-item:last-child {
      padding-bottom: 0;
    }

    .timeline-dot {
      position: absolute;
      left: -24px;
      width: 14px;
      height: 14px;
      border-radius: 50%;
      background: #fff;
      border: 3px solid #667eea;
    }

    .timeline-dot.completed {
      background: #667eea;
    }

    .timeline-content {
      background: #f9f9f9;
      border-radius: 8px;
      padding: 12px 16px;
    }

    .timeline-title {
      font-weight: 500;
      color: #1b264f;
      margin-bottom: 4px;
    }

    .timeline-date {
      font-size: 12px;
      color: #888;
    }

    .timeline-note {
      font-size: 13px;
      color: #666;
      margin-top: 8px;
    }

    /* Refund Summary */
    .refund-summary {
      background: linear-gradient(135deg, #667eea, #764ba2);
      border-radius: 12px;
      padding: 20px;
      color: #fff;
    }

    .refund-summary-title {
      font-size: 12px;
      opacity: 0.8;
      margin-bottom: 8px;
    }

    .refund-summary-amount {
      font-size: 32px;
      font-weight: 700;
    }

    .refund-summary-details {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid rgba(255,255,255,0.2);
      font-size: 13px;
    }

    .refund-detail-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
    }

    /* Empty State */
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      color: #888;
      text-align: center;
      padding: 40px;
    }

    .empty-state uui-icon {
      font-size: 64px;
      margin-bottom: 16px;
      opacity: 0.3;
    }

    .empty-state h3 {
      margin: 0 0 8px 0;
      color: #1b264f;
    }

    .loading {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100%;
    }

    /* Modal */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }

    .modal {
      background: #fff;
      border-radius: 12px;
      width: 90%;
      max-width: 500px;
      overflow: hidden;
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
    }

    .modal-header h3 {
      margin: 0;
      font-size: 18px;
    }

    .modal-body {
      padding: 20px;
    }

    .modal-footer {
      padding: 16px 20px;
      background: #f9f9f9;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }
  `;

  static properties = {
    _returns: { state: true },
    _loading: { state: true },
    _searchTerm: { state: true },
    _statusFilter: { state: true },
    _selectedReturn: { state: true },
    _loadingReturn: { state: true },
    _activeTab: { state: true },
    _processing: { state: true },
    _showRejectModal: { state: true },
    _rejectReason: { state: true },
    _showRefundModal: { state: true },
    _refundAmount: { state: true }
  };

  constructor() {
    super();
    this._returns = [];
    this._loading = true;
    this._searchTerm = '';
    this._statusFilter = 'all';
    this._selectedReturn = null;
    this._loadingReturn = false;
    this._activeTab = 'details';
    this._processing = false;
    this._showRejectModal = false;
    this._rejectReason = '';
    this._showRefundModal = false;
    this._refundAmount = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadReturns();
  }

  async _loadReturns() {
    try {
      this._loading = true;
      const endpoint = this._statusFilter === 'all'
        ? '/umbraco/management/api/v1/ecommerce/returns'
        : `/umbraco/management/api/v1/ecommerce/returns/by-status/${this._statusFilter}`;
      const response = await this._authFetch(endpoint);
      if (response.ok) {
        this._returns = await response.json();
      }
    } catch (error) {
      console.error('Error loading returns:', error);
      // Demo data
      this._returns = [
        { id: '1', returnNumber: 'RET-001', orderId: 'ORD-1234', customerId: 'CUST-001', customerName: 'John Smith', status: 0, reason: 0, requestedAmount: 89.99, requestedAt: '2025-01-20T10:30:00' },
        { id: '2', returnNumber: 'RET-002', orderId: 'ORD-1231', customerId: 'CUST-002', customerName: 'Sarah Johnson', status: 1, reason: 1, requestedAmount: 149.99, approvedAmount: 149.99, requestedAt: '2025-01-19T14:15:00', approvedAt: '2025-01-20T09:00:00' },
        { id: '3', returnNumber: 'RET-003', orderId: 'ORD-1228', customerId: 'CUST-003', customerName: 'Mike Chen', status: 4, reason: 2, requestedAmount: 59.99, approvedAmount: 59.99, refundedAmount: 59.99, requestedAt: '2025-01-15T11:00:00', refundedAt: '2025-01-18T16:30:00' }
      ];
    } finally {
      this._loading = false;
    }
  }

  async _loadReturnDetails(returnId) {
    try {
      this._loadingReturn = true;
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/returns/${returnId}`);
      if (response.ok) {
        this._selectedReturn = await response.json();
      }
    } catch (error) {
      console.error('Error loading return details:', error);
      this._selectedReturn = {
        ...this._returns.find(r => r.id === returnId),
        items: [
          { id: '1', productName: 'Premium Wireless Headphones', productSku: 'WH-1000XM5', quantity: 1, price: 89.99, condition: 0, reason: 'Defective audio in left ear' }
        ],
        timeline: [
          { status: 'Requested', date: '2025-01-20T10:30:00', note: 'Customer initiated return request' }
        ]
      };
    } finally {
      this._loadingReturn = false;
    }
  }

  _handleReturnSelect(ret) {
    this._activeTab = 'details';
    this._loadReturnDetails(ret.id);
  }

  _handleStatusFilterChange(status) {
    this._statusFilter = status;
    this._loadReturns();
  }

  async _handleApprove() {
    if (!confirm('Approve this return request?')) return;

    this._processing = true;
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/approve`, {
        method: 'POST',
        body: JSON.stringify({ approvedAmount: this._selectedReturn.requestedAmount })
      });
      if (!response.ok) throw new Error('Failed to approve');
      this._loadReturnDetails(this._selectedReturn.id);
      this._loadReturns();
    } catch (error) {
      alert('Failed to approve: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  _openRejectModal() {
    this._rejectReason = '';
    this._showRejectModal = true;
  }

  _closeRejectModal() {
    this._showRejectModal = false;
  }

  async _handleReject() {
    if (!this._rejectReason) {
      alert('Please provide a rejection reason');
      return;
    }

    this._processing = true;
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/reject`, {
        method: 'POST',
        body: JSON.stringify({ reason: this._rejectReason })
      });
      if (!response.ok) throw new Error('Failed to reject');
      this._closeRejectModal();
      this._loadReturnDetails(this._selectedReturn.id);
      this._loadReturns();
    } catch (error) {
      alert('Failed to reject: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  async _handleMarkReceived() {
    if (!confirm('Mark items as received?')) return;

    this._processing = true;
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/receive`, {
        method: 'POST'
      });
      if (!response.ok) throw new Error('Failed to update');
      this._loadReturnDetails(this._selectedReturn.id);
      this._loadReturns();
    } catch (error) {
      alert('Failed to update: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  _openRefundModal() {
    this._refundAmount = this._selectedReturn.approvedAmount || this._selectedReturn.requestedAmount;
    this._showRefundModal = true;
  }

  _closeRefundModal() {
    this._showRefundModal = false;
  }

  async _handleProcessRefund() {
    if (this._refundAmount <= 0) {
      alert('Refund amount must be greater than 0');
      return;
    }

    this._processing = true;
    try {
      const response = await this._authFetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/process-refund`, {
        method: 'POST',
        body: JSON.stringify({ amount: this._refundAmount })
      });
      if (!response.ok) throw new Error('Failed to process refund');
      this._closeRefundModal();
      this._loadReturnDetails(this._selectedReturn.id);
      this._loadReturns();
    } catch (error) {
      alert('Failed to process refund: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  _getFilteredReturns() {
    if (!this._searchTerm) return this._returns;
    const term = this._searchTerm.toLowerCase();
    return this._returns.filter(r =>
      r.returnNumber?.toLowerCase().includes(term) ||
      r.customerName?.toLowerCase().includes(term)
    );
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount || 0);
  }

  _getStatusClass(status) {
    switch (status) {
      case 0: return 'pending';
      case 1: return 'approved';
      case 2: return 'rejected';
      case 3: return 'received';
      case 4: return 'refunded';
      default: return 'pending';
    }
  }

  _getStatusText(status) {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Approved';
      case 2: return 'Rejected';
      case 3: return 'Received';
      case 4: return 'Refunded';
      default: return 'Unknown';
    }
  }

  _getReasonText(reason) {
    switch (reason) {
      case 0: return 'Defective';
      case 1: return 'Wrong Item';
      case 2: return 'Not As Described';
      case 3: return 'Changed Mind';
      default: return 'Other';
    }
  }

  _getConditionClass(condition) {
    switch (condition) {
      case 0: return 'unopened';
      case 1: return 'opened';
      case 2: return 'defective';
      default: return 'damaged';
    }
  }

  _getConditionText(condition) {
    switch (condition) {
      case 0: return 'Unopened';
      case 1: return 'Opened';
      case 2: return 'Defective';
      default: return 'Damaged';
    }
  }

  render() {
    return html`
      <div class="list-panel">
        ${this._renderListPanel()}
      </div>
      <div class="editor-panel">
        ${this._selectedReturn ? this._renderEditor() : this._renderEmptyState()}
      </div>
      ${this._showRejectModal ? this._renderRejectModal() : ''}
      ${this._showRefundModal ? this._renderRefundModal() : ''}
    `;
  }

  _renderListPanel() {
    const filteredReturns = this._getFilteredReturns();

    return html`
      <div class="list-header">
        <h2>Returns & Refunds</h2>
        <div class="search-box">
          <uui-icon name="icon-search"></uui-icon>
          <input type="text" placeholder="Search returns..." .value=${this._searchTerm} @input=${(e) => this._searchTerm = e.target.value} />
        </div>
      </div>

      <div class="status-filters">
        <span class="status-chip all ${this._statusFilter === 'all' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('all')}>All</span>
        <span class="status-chip pending ${this._statusFilter === '0' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('0')}>Pending</span>
        <span class="status-chip approved ${this._statusFilter === '1' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('1')}>Approved</span>
        <span class="status-chip received ${this._statusFilter === '3' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('3')}>Received</span>
        <span class="status-chip refunded ${this._statusFilter === '4' ? 'active' : ''}" @click=${() => this._handleStatusFilterChange('4')}>Refunded</span>
      </div>

      <div class="return-list">
        ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
          filteredReturns.length === 0 ? html`
            <div class="empty-state">
              <uui-icon name="icon-undo"></uui-icon>
              <p>No returns found</p>
            </div>
          ` : filteredReturns.map(ret => html`
            <div class="return-item ${this._selectedReturn?.id === ret.id ? 'selected' : ''}" @click=${() => this._handleReturnSelect(ret)}>
              <div class="return-icon ${this._getStatusClass(ret.status)}">
                <uui-icon name="icon-undo"></uui-icon>
              </div>
              <div class="return-info">
                <div class="return-number">${ret.returnNumber}</div>
                <div class="return-meta">${ret.customerName || 'Guest'} • ${this._getStatusText(ret.status)}</div>
              </div>
              <div class="return-amount">
                <div class="amount-value">${this._formatCurrency(ret.requestedAmount)}</div>
                <div class="amount-label">requested</div>
              </div>
            </div>
          `)}
      </div>
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-undo"></uui-icon>
        <h3>Select a Return</h3>
        <p>Select a return request from the list to view details and process the refund.</p>
      </div>
    `;
  }

  _renderEditor() {
    if (this._loadingReturn) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    const ret = this._selectedReturn;

    return html`
      <div class="editor-header">
        <div>
          <h2>${ret.returnNumber}</h2>
          <span class="status-badge ${this._getStatusClass(ret.status)}">${this._getStatusText(ret.status)}</span>
        </div>
        <div class="editor-actions">
          <uui-button look="secondary" @click=${() => this._selectedReturn = null}>Close</uui-button>
        </div>
      </div>

      <div class="tabs">
        <div class="tab ${this._activeTab === 'details' ? 'active' : ''}" @click=${() => this._activeTab = 'details'}>Details</div>
        <div class="tab ${this._activeTab === 'items' ? 'active' : ''}" @click=${() => this._activeTab = 'items'}>Items</div>
        <div class="tab ${this._activeTab === 'timeline' ? 'active' : ''}" @click=${() => this._activeTab = 'timeline'}>Timeline</div>
        <div class="tab ${this._activeTab === 'refund' ? 'active' : ''}" @click=${() => this._activeTab = 'refund'}>Refund</div>
      </div>

      <div class="tab-content">
        ${this._activeTab === 'details' ? this._renderDetailsTab(ret) :
          this._activeTab === 'items' ? this._renderItemsTab(ret) :
          this._activeTab === 'timeline' ? this._renderTimelineTab(ret) :
          this._renderRefundTab(ret)}
      </div>
    `;
  }

  _renderDetailsTab(ret) {
    return html`
      ${ret.status === 0 ? html`
        <div class="quick-actions">
          <button class="action-btn primary" @click=${this._handleApprove} ?disabled=${this._processing}>
            <uui-icon name="icon-check"></uui-icon> Approve Return
          </button>
          <button class="action-btn danger" @click=${this._openRejectModal} ?disabled=${this._processing}>
            <uui-icon name="icon-delete"></uui-icon> Reject
          </button>
        </div>
      ` : ret.status === 1 ? html`
        <div class="quick-actions">
          <button class="action-btn secondary" @click=${this._handleMarkReceived} ?disabled=${this._processing}>
            <uui-icon name="icon-box"></uui-icon> Mark as Received
          </button>
          <button class="action-btn primary" @click=${this._openRefundModal} ?disabled=${this._processing}>
            <uui-icon name="icon-coins"></uui-icon> Process Refund
          </button>
        </div>
      ` : ret.status === 3 ? html`
        <div class="quick-actions">
          <button class="action-btn primary" @click=${this._openRefundModal} ?disabled=${this._processing}>
            <uui-icon name="icon-coins"></uui-icon> Process Refund
          </button>
        </div>
      ` : ''}

      <div class="info-cards">
        <div class="info-card">
          <div class="info-card-label">Requested Amount</div>
          <div class="info-card-value requested">${this._formatCurrency(ret.requestedAmount)}</div>
        </div>
        <div class="info-card">
          <div class="info-card-label">Approved Amount</div>
          <div class="info-card-value approved">${this._formatCurrency(ret.approvedAmount || 0)}</div>
        </div>
        <div class="info-card">
          <div class="info-card-label">Refunded</div>
          <div class="info-card-value refunded">${this._formatCurrency(ret.refundedAmount || 0)}</div>
        </div>
        <div class="info-card">
          <div class="info-card-label">Reason</div>
          <div class="info-card-value" style="font-size:16px;">${this._getReasonText(ret.reason)}</div>
        </div>
      </div>

      <div class="form-section">
        <div class="form-section-title">Return Information</div>
        <div class="form-grid">
          <div class="form-group">
            <label>Return Number</label>
            <input type="text" readonly .value=${ret.returnNumber} />
          </div>
          <div class="form-group">
            <label>Order ID</label>
            <input type="text" readonly .value=${ret.orderId} />
          </div>
          <div class="form-group">
            <label>Customer</label>
            <input type="text" readonly .value=${ret.customerName || 'Guest'} />
          </div>
          <div class="form-group">
            <label>Requested At</label>
            <input type="text" readonly .value=${ret.requestedAt ? new Date(ret.requestedAt).toLocaleString() : '-'} />
          </div>
        </div>
      </div>

      ${ret.customerNotes ? html`
        <div class="form-section">
          <div class="form-section-title">Customer Notes</div>
          <p style="color:#666;margin:0;">${ret.customerNotes}</p>
        </div>
      ` : ''}
    `;
  }

  _renderItemsTab(ret) {
    const items = ret.items || [];

    return html`
      <div class="form-section">
        <div class="form-section-title">Return Items</div>
        ${items.length === 0 ? html`
          <div class="empty-state" style="padding:40px;">
            <p>No items information available</p>
          </div>
        ` : html`
          <table class="items-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Qty</th>
                <th>Price</th>
                <th>Condition</th>
                <th>Reason</th>
              </tr>
            </thead>
            <tbody>
              ${items.map(item => html`
                <tr>
                  <td>
                    <div class="item-product">
                      <div class="item-image"><uui-icon name="icon-box"></uui-icon></div>
                      <div>
                        <div class="item-name">${item.productName}</div>
                        <div class="item-sku">${item.productSku}</div>
                      </div>
                    </div>
                  </td>
                  <td>${item.quantity}</td>
                  <td>${this._formatCurrency(item.price)}</td>
                  <td><span class="condition-badge ${this._getConditionClass(item.condition)}">${this._getConditionText(item.condition)}</span></td>
                  <td>${item.reason || '-'}</td>
                </tr>
              `)}
            </tbody>
          </table>
        `}
      </div>
    `;
  }

  _renderTimelineTab(ret) {
    const timeline = ret.timeline || [
      { status: 'Requested', date: ret.requestedAt, note: 'Customer initiated return request' }
    ];

    if (ret.approvedAt) {
      timeline.push({ status: 'Approved', date: ret.approvedAt, note: 'Return request approved' });
    }
    if (ret.receivedAt) {
      timeline.push({ status: 'Received', date: ret.receivedAt, note: 'Items received at warehouse' });
    }
    if (ret.refundedAt) {
      timeline.push({ status: 'Refunded', date: ret.refundedAt, note: `Refund of ${this._formatCurrency(ret.refundedAmount)} processed` });
    }

    return html`
      <div class="form-section">
        <div class="form-section-title">Status Timeline</div>
        <div class="timeline">
          ${timeline.map((event, index) => html`
            <div class="timeline-item">
              <div class="timeline-dot ${index < timeline.length - 1 ? 'completed' : ''}"></div>
              <div class="timeline-content">
                <div class="timeline-title">${event.status}</div>
                <div class="timeline-date">${event.date ? new Date(event.date).toLocaleString() : '-'}</div>
                ${event.note ? html`<div class="timeline-note">${event.note}</div>` : ''}
              </div>
            </div>
          `)}
        </div>
      </div>
    `;
  }

  _renderRefundTab(ret) {
    return html`
      <div class="refund-summary">
        <div class="refund-summary-title">REFUND SUMMARY</div>
        <div class="refund-summary-amount">${this._formatCurrency(ret.refundedAmount || ret.approvedAmount || ret.requestedAmount)}</div>
        <div class="refund-summary-details">
          <div class="refund-detail-row">
            <span>Requested Amount</span>
            <span>${this._formatCurrency(ret.requestedAmount)}</span>
          </div>
          <div class="refund-detail-row">
            <span>Approved Amount</span>
            <span>${this._formatCurrency(ret.approvedAmount || 0)}</span>
          </div>
          <div class="refund-detail-row">
            <span>Refunded Amount</span>
            <span>${this._formatCurrency(ret.refundedAmount || 0)}</span>
          </div>
        </div>
      </div>

      ${ret.status !== 4 && ret.status !== 2 ? html`
        <div class="form-section" style="margin-top:20px;">
          <div class="form-section-title">Process Refund</div>
          <p style="color:#666;font-size:14px;margin-bottom:16px;">
            ${ret.status === 0 ? 'Approve the return first before processing the refund.' :
              'Click the button below to process the refund for this return.'}
          </p>
          ${ret.status !== 0 ? html`
            <button class="action-btn primary" @click=${this._openRefundModal} ?disabled=${this._processing}>
              <uui-icon name="icon-coins"></uui-icon> Process Refund
            </button>
          ` : ''}
        </div>
      ` : ret.status === 4 ? html`
        <div class="form-section" style="margin-top:20px;">
          <div class="form-section-title">Refund Complete</div>
          <p style="color:#22c55e;font-size:14px;margin:0;">
            <uui-icon name="icon-check"></uui-icon> Refund has been successfully processed on ${ret.refundedAt ? new Date(ret.refundedAt).toLocaleString() : 'N/A'}.
          </p>
        </div>
      ` : ''}
    `;
  }

  _renderRejectModal() {
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeRejectModal()}>
        <div class="modal">
          <div class="modal-header">
            <h3>Reject Return</h3>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Rejection Reason *</label>
              <textarea rows="4" .value=${this._rejectReason} @input=${(e) => this._rejectReason = e.target.value} placeholder="Please provide a reason for rejecting this return..."></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeRejectModal}>Cancel</uui-button>
            <uui-button look="primary" color="danger" @click=${this._handleReject} ?disabled=${this._processing}>Reject Return</uui-button>
          </div>
        </div>
      </div>
    `;
  }

  _renderRefundModal() {
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeRefundModal()}>
        <div class="modal">
          <div class="modal-header">
            <h3>Process Refund</h3>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Refund Amount</label>
              <input type="number" step="0.01" min="0.01" .value=${this._refundAmount} @input=${(e) => this._refundAmount = parseFloat(e.target.value)} />
              <p style="font-size:12px;color:#888;margin-top:4px;">Maximum: ${this._formatCurrency(this._selectedReturn?.approvedAmount || this._selectedReturn?.requestedAmount)}</p>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeRefundModal}>Cancel</uui-button>
            <uui-button look="primary" @click=${this._handleProcessRefund} ?disabled=${this._processing}>Process Refund</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-return-collection', ReturnCollection);
export default ReturnCollection;
