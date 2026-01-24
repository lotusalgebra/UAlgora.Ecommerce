import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Payments View
 * Displays payment history and allows processing refunds.
 */
export class OrderPayments extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .payments-container {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-layout-1);
    }

    .payment-summary {
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .summary-item {
      text-align: center;
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
    }

    .summary-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
      margin-bottom: var(--uui-size-space-2);
    }

    .summary-value {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
    }

    .summary-value.positive { color: var(--uui-color-positive); }
    .summary-value.warning { color: var(--uui-color-warning); }
    .summary-value.danger { color: var(--uui-color-danger); }

    .payment-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .payment-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-4);
      padding-bottom: var(--uui-size-space-4);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .payment-amount {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
    }

    .payment-amount.refund {
      color: var(--uui-color-danger);
    }

    .status-badge {
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-weight: 500;
      font-size: var(--uui-type-small-size);
      text-transform: uppercase;
    }

    .status-pending { background: #ffc107; color: #000; }
    .status-authorized { background: #17a2b8; color: #fff; }
    .status-captured { background: #28a745; color: #fff; }
    .status-failed { background: #dc3545; color: #fff; }
    .status-refunded { background: #fd7e14; color: #fff; }
    .status-partiallyrefunded { background: #6f42c1; color: #fff; }
    .status-voided { background: #6c757d; color: #fff; }

    .payment-details {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--uui-size-space-4);
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-1);
    }

    .detail-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .detail-value {
      font-weight: 500;
    }

    .card-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .card-brand {
      font-weight: bold;
      text-transform: capitalize;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .refund-section {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .refund-form {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-4);
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 2fr;
      gap: var(--uui-size-space-4);
      align-items: start;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
    }

    .form-group label {
      font-weight: 500;
    }

    .form-actions {
      display: flex;
      gap: var(--uui-size-space-2);
      justify-content: flex-end;
    }

    .refund-type-options {
      display: flex;
      gap: var(--uui-size-space-4);
    }

    .refund-warning {
      background: var(--uui-color-warning);
      color: #000;
      padding: var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _payments: { type: Array, state: true },
    _refundEligibility: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _showRefundForm: { type: Boolean, state: true },
    _refundAmount: { type: Number, state: true },
    _refundReason: { type: String, state: true },
    _refundType: { type: String, state: true },
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._order = null;
    this._payments = [];
    this._refundEligibility = null;
    this._loading = true;
    this._showRefundForm = false;
    this._refundAmount = 0;
    this._refundReason = '';
    this._refundType = 'full';
    this._processing = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
      await Promise.all([
        this._loadPayments(),
        this._loadRefundEligibility()
      ]);
    }
    this._loading = false;
  }

  async _loadPayments() {
    if (!this._order?.id) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/payments`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._payments = await response.json();
      }
    } catch (error) {
      console.error('Error loading payments:', error);
    }
  }

  async _loadRefundEligibility() {
    if (!this._order?.id) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/can-refund`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        this._refundEligibility = await response.json();
        this._refundAmount = this._refundEligibility.refundableAmount;
      }
    } catch (error) {
      console.error('Error checking refund eligibility:', error);
    }
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode
    }).format(amount || 0);
  }

  _formatDate(date) {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  _getStatusClass(status) {
    return `status-${status?.toLowerCase().replace(/\s+/g, '') || 'pending'}`;
  }

  _toggleRefundForm() {
    this._showRefundForm = !this._showRefundForm;
    if (this._showRefundForm) {
      this._refundType = 'full';
      this._refundAmount = this._refundEligibility?.refundableAmount || 0;
      this._refundReason = '';
    }
  }

  _handleRefundTypeChange(type) {
    this._refundType = type;
    if (type === 'full') {
      this._refundAmount = this._refundEligibility?.refundableAmount || 0;
    }
  }

  async _processRefund() {
    if (!this._order?.id) return;

    if (this._refundAmount <= 0) {
      alert('Please enter a valid refund amount');
      return;
    }

    if (this._refundAmount > this._refundEligibility?.refundableAmount) {
      alert(`Maximum refundable amount is ${this._formatCurrency(this._refundEligibility.refundableAmount, this._order.currencyCode)}`);
      return;
    }

    if (!confirm(`Are you sure you want to refund ${this._formatCurrency(this._refundAmount, this._order.currencyCode)}?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${this._order.id}/refund`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          amount: this._refundAmount,
          reason: this._refundReason
        })
      });

      if (response.ok) {
        this._showRefundForm = false;
        await Promise.all([
          this._loadPayments(),
          this._loadRefundEligibility()
        ]);

        // Refresh the workspace
        const workspace = this.closest('ecommerce-order-workspace');
        if (workspace) {
          await workspace.refreshOrder();
          this._order = workspace.getOrder();
        }
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to process refund');
      }
    } catch (error) {
      console.error('Error processing refund:', error);
      alert('Failed to process refund');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (this._loading) {
      return html`<uui-loader></uui-loader>`;
    }

    if (!this._order) {
      return html`<p>Order not found</p>`;
    }

    const currency = this._order.currencyCode || 'USD';

    return html`
      <div class="payments-container">
        ${this._renderPaymentSummary(currency)}
        ${this._refundEligibility?.canRefund ? this._renderRefundSection(currency) : ''}
        ${this._renderPaymentsList(currency)}
      </div>
    `;
  }

  _renderPaymentSummary(currency) {
    const paid = this._order.paidAmount || 0;
    const refunded = this._order.refundedAmount || 0;
    const balance = this._order.grandTotal - paid;

    return html`
      <div class="payment-summary">
        <div class="summary-grid">
          <div class="summary-item">
            <div class="summary-label">Order Total</div>
            <div class="summary-value">${this._formatCurrency(this._order.grandTotal, currency)}</div>
          </div>
          <div class="summary-item">
            <div class="summary-label">Amount Paid</div>
            <div class="summary-value positive">${this._formatCurrency(paid, currency)}</div>
          </div>
          <div class="summary-item">
            <div class="summary-label">Amount Refunded</div>
            <div class="summary-value ${refunded > 0 ? 'warning' : ''}">${this._formatCurrency(refunded, currency)}</div>
          </div>
          <div class="summary-item">
            <div class="summary-label">Balance Due</div>
            <div class="summary-value ${balance > 0 ? 'danger' : 'positive'}">
              ${this._formatCurrency(balance, currency)}
            </div>
          </div>
        </div>
      </div>
    `;
  }

  _renderRefundSection(currency) {
    return html`
      <div class="refund-section">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--uui-size-space-4);">
          <h3 style="margin: 0;">Refunds</h3>
          ${!this._showRefundForm ? html`
            <uui-button look="primary" color="warning" @click=${this._toggleRefundForm}>
              <uui-icon name="icon-coin-dollar"></uui-icon>
              Process Refund
            </uui-button>
          ` : ''}
        </div>

        ${this._showRefundForm ? html`
          <div class="refund-warning">
            <strong>Warning:</strong> Refunds are permanent and cannot be undone.
          </div>

          <div class="refund-form">
            <div class="form-group">
              <label>Refund Type</label>
              <div class="refund-type-options">
                <uui-radio-group
                  .value=${this._refundType}
                  @change=${(e) => this._handleRefundTypeChange(e.target.value)}
                >
                  <uui-radio value="full" label="Full Refund (${this._formatCurrency(this._refundEligibility?.refundableAmount, currency)})"></uui-radio>
                  <uui-radio value="partial" label="Partial Refund"></uui-radio>
                </uui-radio-group>
              </div>
            </div>

            ${this._refundType === 'partial' ? html`
              <div class="form-row">
                <div class="form-group">
                  <label>Refund Amount</label>
                  <uui-input
                    type="number"
                    step="0.01"
                    min="0.01"
                    max=${this._refundEligibility?.refundableAmount}
                    .value=${this._refundAmount}
                    @input=${(e) => this._refundAmount = parseFloat(e.target.value) || 0}
                  ></uui-input>
                  <span style="font-size: var(--uui-type-small-size); color: var(--uui-color-text-alt);">
                    Maximum: ${this._formatCurrency(this._refundEligibility?.refundableAmount, currency)}
                  </span>
                </div>
              </div>
            ` : ''}

            <div class="form-group">
              <label>Reason (optional)</label>
              <uui-textarea
                .value=${this._refundReason}
                @input=${(e) => this._refundReason = e.target.value}
                placeholder="Enter the reason for this refund..."
                rows="3"
              ></uui-textarea>
            </div>

            <div class="form-actions">
              <uui-button look="secondary" @click=${this._toggleRefundForm}>
                Cancel
              </uui-button>
              <uui-button
                look="primary"
                color="warning"
                ?disabled=${this._processing}
                @click=${this._processRefund}
              >
                ${this._processing ? 'Processing...' : `Refund ${this._formatCurrency(this._refundAmount, currency)}`}
              </uui-button>
            </div>
          </div>
        ` : html`
          <p style="color: var(--uui-color-text-alt); margin: 0;">
            Refundable amount: <strong>${this._formatCurrency(this._refundEligibility?.refundableAmount, currency)}</strong>
          </p>
        `}
      </div>
    `;
  }

  _renderPaymentsList(currency) {
    if (this._payments.length === 0) {
      return html`
        <uui-box>
          <div slot="headline">Payment History</div>
          <div class="empty-state">
            <uui-icon name="icon-credit-card" style="font-size: 48px;"></uui-icon>
            <p>No payment transactions recorded</p>
          </div>
        </uui-box>
      `;
    }

    return html`
      <uui-box>
        <div slot="headline">Payment History (${this._payments.length})</div>
        ${this._payments.map(payment => this._renderPayment(payment, currency))}
      </uui-box>
    `;
  }

  _renderPayment(payment, currency) {
    return html`
      <div class="payment-card">
        <div class="payment-header">
          <div>
            <span class="payment-amount ${payment.isRefund ? 'refund' : ''}">
              ${payment.isRefund ? '-' : ''}${this._formatCurrency(payment.amount, payment.currencyCode || currency)}
            </span>
            <span style="color: var(--uui-color-text-alt); margin-left: var(--uui-size-space-2);">
              ${payment.isRefund ? 'Refund' : 'Payment'}
            </span>
          </div>
          <span class="status-badge ${this._getStatusClass(payment.status)}">
            ${payment.status}
          </span>
        </div>

        <div class="payment-details">
          <div class="detail-item">
            <span class="detail-label">Date</span>
            <span class="detail-value">${this._formatDate(payment.createdAt)}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Provider</span>
            <span class="detail-value">${payment.provider || 'Unknown'}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Method</span>
            <span class="detail-value">
              ${payment.cardBrand ? html`
                <span class="card-info">
                  <span class="card-brand">${payment.cardBrand}</span>
                  ${payment.cardLast4 ? html`<span>•••• ${payment.cardLast4}</span>` : ''}
                </span>
              ` : payment.methodName || payment.methodType || 'N/A'}
            </span>
          </div>
          <div class="detail-item">
            <span class="detail-label">Transaction ID</span>
            <span class="detail-value" style="font-family: monospace; font-size: var(--uui-type-small-size);">
              ${payment.transactionId || 'N/A'}
            </span>
          </div>
          ${payment.capturedAt ? html`
            <div class="detail-item">
              <span class="detail-label">Captured</span>
              <span class="detail-value">${this._formatDate(payment.capturedAt)}</span>
            </div>
          ` : ''}
          ${payment.refundedAt ? html`
            <div class="detail-item">
              <span class="detail-label">Refunded</span>
              <span class="detail-value">${this._formatDate(payment.refundedAt)}</span>
            </div>
          ` : ''}
          ${payment.errorMessage ? html`
            <div class="detail-item" style="grid-column: 1 / -1;">
              <span class="detail-label">Error</span>
              <span class="detail-value" style="color: var(--uui-color-danger);">${payment.errorMessage}</span>
            </div>
          ` : ''}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-order-payments', OrderPayments);

export default OrderPayments;
