import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class ReturnCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; padding: 20px; }
    .collection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; gap: 16px; }
    .filter-group { display: flex; gap: 8px; }
    .collection-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; }
    .collection-table th, .collection-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .collection-table th { background: #f5f5f5; font-weight: 600; }
    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .status-pending { background: #fff3cd; color: #856404; }
    .status-approved { background: #d4edda; color: #155724; }
    .status-rejected { background: #f8d7da; color: #721c24; }
    .status-received { background: #cce5ff; color: #004085; }
    .status-refunded { background: #d1ecf1; color: #0c5460; }
    .amount { font-weight: 600; }
    .empty-state { text-align: center; padding: 60px 20px; color: #666; }
    .loading { display: flex; justify-content: center; padding: 40px; }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 10000; }
    .modal { background: white; border-radius: 8px; width: 90%; max-width: 600px; max-height: 90vh; overflow-y: auto; }
    .modal-header { padding: 20px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; }
    .modal-header h2 { margin: 0; }
    .modal-body { padding: 20px; }
    .modal-footer { padding: 20px; border-top: 1px solid #e0e0e0; display: flex; justify-content: flex-end; gap: 10px; }
    .detail-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }
    .detail-label { color: #666; }
    .detail-value { font-weight: 500; }
    .action-buttons { display: flex; gap: 8px; margin-top: 16px; }
  `;

  static properties = {
    _returns: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _statusFilter: { type: String, state: true },
    _showModal: { type: Boolean, state: true },
    _selectedReturn: { type: Object, state: true },
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._returns = [];
    this._loading = true;
    this._statusFilter = 'pending';
    this._showModal = false;
    this._selectedReturn = null;
    this._processing = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadReturns();
  }

  async _loadReturns() {
    try {
      this._loading = true;
      const endpoint = this._statusFilter === 'pending'
        ? '/umbraco/management/api/v1/ecommerce/returns/pending'
        : `/umbraco/management/api/v1/ecommerce/returns/by-status/${this._statusFilter}`;
      const response = await fetch(endpoint, {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._returns = await response.json();
      }
    } catch (error) {
      console.error('Error loading returns:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleFilterChange(status) {
    this._statusFilter = status;
    this._loadReturns();
  }

  _openDetailModal(ret) {
    this._selectedReturn = ret;
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._selectedReturn = null;
  }

  async _approveReturn() {
    if (!confirm('Approve this return request?')) return;
    this._processing = true;
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/approve`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ notes: 'Approved via backoffice' })
      });
      if (!response.ok) throw new Error('Failed to approve');
      this._closeModal();
      this._loadReturns();
    } catch (error) {
      alert('Failed to approve: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  async _rejectReturn() {
    const reason = prompt('Enter rejection reason:');
    if (!reason) return;
    this._processing = true;
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/reject`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reason })
      });
      if (!response.ok) throw new Error('Failed to reject');
      this._closeModal();
      this._loadReturns();
    } catch (error) {
      alert('Failed to reject: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  async _processRefund() {
    if (!confirm('Process refund for this return?')) return;
    this._processing = true;
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/returns/${this._selectedReturn.id}/process-refund`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' }
      });
      if (!response.ok) throw new Error('Failed to process refund');
      this._closeModal();
      this._loadReturns();
    } catch (error) {
      alert('Failed to process refund: ' + error.message);
    } finally {
      this._processing = false;
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount || 0);
  }

  _getStatusClass(status) {
    const classes = { 0: 'status-pending', 1: 'status-approved', 2: 'status-rejected', 3: 'status-received', 4: 'status-refunded' };
    return classes[status] || 'status-pending';
  }

  _getStatusText(status) {
    const texts = { 0: 'Pending', 1: 'Approved', 2: 'Rejected', 3: 'Received', 4: 'Refunded' };
    return texts[status] || 'Unknown';
  }

  render() {
    return html`
      <div class="collection-header">
        <h2>Returns & Refunds</h2>
        <div class="filter-group">
          <uui-button look=${this._statusFilter === 'pending' ? 'primary' : 'secondary'} compact @click=${() => this._handleFilterChange('pending')}>Pending</uui-button>
          <uui-button look=${this._statusFilter === '1' ? 'primary' : 'secondary'} compact @click=${() => this._handleFilterChange('1')}>Approved</uui-button>
          <uui-button look=${this._statusFilter === '4' ? 'primary' : 'secondary'} compact @click=${() => this._handleFilterChange('4')}>Refunded</uui-button>
        </div>
      </div>
      ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
        this._returns.length === 0 ? html`<div class="empty-state"><uui-icon name="icon-undo" style="font-size:48px;"></uui-icon><h3>No returns</h3><p>No return requests found</p></div>` :
        html`<table class="collection-table">
          <thead><tr><th>Return #</th><th>Order</th><th>Customer</th><th>Reason</th><th>Amount</th><th>Status</th><th>Date</th></tr></thead>
          <tbody>${this._returns.map(r => html`
            <tr @click=${() => this._openDetailModal(r)}>
              <td><strong>${r.returnNumber}</strong></td>
              <td>${r.orderId ? r.orderId.substring(0, 8) + '...' : '-'}</td>
              <td>${r.customerId ? r.customerId.substring(0, 8) + '...' : 'Guest'}</td>
              <td>${this._getReasonText(r.reason)}</td>
              <td class="amount">${this._formatCurrency(r.requestedAmount)}</td>
              <td><span class="status-badge ${this._getStatusClass(r.status)}">${this._getStatusText(r.status)}</span></td>
              <td>${r.requestedAt ? new Date(r.requestedAt).toLocaleDateString() : '-'}</td>
            </tr>
          `)}</tbody>
        </table>`}
      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _getReasonText(reason) {
    const reasons = { 0: 'Defective', 1: 'Wrong Item', 2: 'Not As Described', 3: 'Changed Mind', 4: 'Other' };
    return reasons[reason] || 'Unknown';
  }

  _renderModal() {
    const r = this._selectedReturn;
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header"><h2>Return ${r.returnNumber}</h2><uui-button look="secondary" compact @click=${this._closeModal}>&times;</uui-button></div>
          <div class="modal-body">
            <div class="detail-row"><span class="detail-label">Status</span><span class="status-badge ${this._getStatusClass(r.status)}">${this._getStatusText(r.status)}</span></div>
            <div class="detail-row"><span class="detail-label">Reason</span><span class="detail-value">${this._getReasonText(r.reason)}</span></div>
            <div class="detail-row"><span class="detail-label">Requested Amount</span><span class="detail-value amount">${this._formatCurrency(r.requestedAmount)}</span></div>
            ${r.approvedAmount ? html`<div class="detail-row"><span class="detail-label">Approved Amount</span><span class="detail-value amount">${this._formatCurrency(r.approvedAmount)}</span></div>` : ''}
            <div class="detail-row"><span class="detail-label">Requested At</span><span class="detail-value">${r.requestedAt ? new Date(r.requestedAt).toLocaleString() : '-'}</span></div>
            ${r.customerNotes ? html`<div class="detail-row"><span class="detail-label">Customer Notes</span><span class="detail-value">${r.customerNotes}</span></div>` : ''}
            ${r.status === 0 ? html`
              <div class="action-buttons">
                <uui-button look="primary" @click=${this._approveReturn} ?disabled=${this._processing}>Approve</uui-button>
                <uui-button look="secondary" color="danger" @click=${this._rejectReturn} ?disabled=${this._processing}>Reject</uui-button>
              </div>
            ` : ''}
            ${r.status === 1 || r.status === 3 ? html`
              <div class="action-buttons">
                <uui-button look="primary" @click=${this._processRefund} ?disabled=${this._processing}>Process Refund</uui-button>
              </div>
            ` : ''}
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeModal}>Close</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-return-collection', ReturnCollection);
export default ReturnCollection;
