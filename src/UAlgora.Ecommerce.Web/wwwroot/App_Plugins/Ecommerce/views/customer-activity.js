import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Activity View
 * Displays loyalty points, store credit, and customer activity.
 */
export class CustomerActivity extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .activity-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .activity-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
      padding-bottom: var(--uui-size-space-4);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .card-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .card-title uui-icon {
      color: var(--uui-color-interactive);
    }

    .big-value {
      font-size: var(--uui-type-h1-size);
      font-weight: bold;
      color: var(--uui-color-interactive);
      margin-bottom: var(--uui-size-space-2);
    }

    .big-value.credit {
      color: var(--uui-color-positive);
    }

    .big-value.points {
      color: #ffc107;
    }

    .value-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .tier-badge {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      border-radius: var(--uui-border-radius);
      font-weight: 500;
    }

    .tier-standard {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text);
    }

    .tier-bronze {
      background: #cd7f32;
      color: #fff;
    }

    .tier-silver {
      background: #c0c0c0;
      color: #000;
    }

    .tier-gold {
      background: #ffc107;
      color: #000;
    }

    .tier-vip {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: #fff;
    }

    .action-buttons {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-layout-1);
    }

    .adjustment-form {
      margin-top: var(--uui-size-layout-1);
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
    }

    .form-row {
      display: flex;
      gap: var(--uui-size-space-3);
      align-items: flex-end;
      margin-bottom: var(--uui-size-space-3);
    }

    .form-group {
      flex: 1;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
      font-size: var(--uui-type-small-size);
    }

    .form-group uui-input {
      width: 100%;
    }

    .info-list {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .info-list li {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-3) 0;
      border-bottom: 1px solid var(--uui-color-border-standalone);
    }

    .info-list li:last-child {
      border-bottom: none;
    }

    .info-label {
      color: var(--uui-color-text-alt);
    }

    .info-value {
      font-weight: 500;
    }

    .timeline {
      margin-top: var(--uui-size-layout-1);
    }

    .timeline-item {
      display: flex;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3) 0;
      border-bottom: 1px solid var(--uui-color-border-standalone);
    }

    .timeline-item:last-child {
      border-bottom: none;
    }

    .timeline-icon {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .timeline-icon.positive {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .timeline-icon.negative {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .timeline-content {
      flex: 1;
    }

    .timeline-title {
      font-weight: 500;
      margin-bottom: var(--uui-size-space-1);
    }

    .timeline-date {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-2);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _customer: { type: Object, state: true },
    _showPointsForm: { type: Boolean, state: true },
    _showCreditForm: { type: Boolean, state: true },
    _pointsAmount: { type: Number, state: true },
    _creditAmount: { type: Number, state: true },
    _adjustmentReason: { type: String, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._customer = null;
    this._showPointsForm = false;
    this._showCreditForm = false;
    this._pointsAmount = 0;
    this._creditAmount = 0;
    this._adjustmentReason = '';
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      this._customer = workspace.getCustomer();
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount || 0);
  }

  _getTierClass(tier) {
    const t = tier?.toLowerCase() || 'standard';
    if (t === 'vip') return 'tier-vip';
    if (t === 'gold') return 'tier-gold';
    if (t === 'silver') return 'tier-silver';
    if (t === 'bronze') return 'tier-bronze';
    return 'tier-standard';
  }

  async _handleAddPoints() {
    if (!this._customer?.id || !this._pointsAmount) return;

    try {
      this._saving = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${this._customer.id}/loyalty-points`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          points: this._pointsAmount,
          reason: this._adjustmentReason
        })
      });

      if (!response.ok) {
        throw new Error('Failed to update loyalty points');
      }

      const updatedCustomer = await response.json();
      this._updateWorkspace(updatedCustomer);
      this._showPointsForm = false;
      this._pointsAmount = 0;
      this._adjustmentReason = '';
      this._showNotification('positive', 'Loyalty points updated successfully');
    } catch (error) {
      console.error('Error updating points:', error);
      this._showNotification('danger', 'Failed to update loyalty points');
    } finally {
      this._saving = false;
    }
  }

  async _handleAddCredit() {
    if (!this._customer?.id || !this._creditAmount) return;

    try {
      this._saving = true;
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${this._customer.id}/store-credit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          amount: this._creditAmount,
          reason: this._adjustmentReason
        })
      });

      if (!response.ok) {
        throw new Error('Failed to update store credit');
      }

      const updatedCustomer = await response.json();
      this._updateWorkspace(updatedCustomer);
      this._showCreditForm = false;
      this._creditAmount = 0;
      this._adjustmentReason = '';
      this._showNotification('positive', 'Store credit updated successfully');
    } catch (error) {
      console.error('Error updating credit:', error);
      this._showNotification('danger', 'Failed to update store credit');
    } finally {
      this._saving = false;
    }
  }

  _updateWorkspace(customer) {
    const workspace = this.closest('ecommerce-customer-workspace');
    if (workspace) {
      workspace.setCustomer(customer);
      this._customer = customer;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: { headline: color === 'positive' ? 'Success' : 'Error', message, color }
    });
    this.dispatchEvent(event);
  }

  render() {
    const isNewCustomer = !this._customer?.id;

    if (isNewCustomer) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-chart-line" style="font-size: 48px;"></uui-icon>
          <h3>No activity yet</h3>
          <p>Save the customer first to track their activity.</p>
        </div>
      `;
    }

    return html`
      <div class="activity-grid">
        <!-- Loyalty Points Card -->
        <div class="activity-card">
          <div class="card-header">
            <span class="card-title">
              <uui-icon name="icon-medal"></uui-icon>
              Loyalty Points
            </span>
            <span class="tier-badge ${this._getTierClass(this._customer?.customerTier)}">
              ${this._customer?.customerTier || 'Standard'}
            </span>
          </div>
          <div class="big-value points">
            ${(this._customer?.loyaltyPoints || 0).toLocaleString()}
          </div>
          <div class="value-label">Available Points</div>

          ${this._showPointsForm ? html`
            <div class="adjustment-form">
              <div class="form-row">
                <div class="form-group">
                  <label>Points (+/-)</label>
                  <uui-input
                    type="number"
                    .value=${this._pointsAmount}
                    @input=${(e) => this._pointsAmount = parseInt(e.target.value) || 0}
                    placeholder="Enter amount"
                  ></uui-input>
                </div>
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label>Reason</label>
                  <uui-input
                    .value=${this._adjustmentReason}
                    @input=${(e) => this._adjustmentReason = e.target.value}
                    placeholder="Enter reason for adjustment"
                  ></uui-input>
                </div>
              </div>
              <div class="action-buttons">
                <uui-button look="secondary" @click=${() => this._showPointsForm = false}>
                  Cancel
                </uui-button>
                <uui-button
                  look="primary"
                  ?disabled=${this._saving || !this._pointsAmount}
                  @click=${this._handleAddPoints}
                >
                  ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : 'Apply'}
                </uui-button>
              </div>
            </div>
          ` : html`
            <div class="action-buttons">
              <uui-button look="secondary" @click=${() => this._showPointsForm = true}>
                <uui-icon name="icon-add"></uui-icon>
                Adjust Points
              </uui-button>
            </div>
          `}
        </div>

        <!-- Store Credit Card -->
        <div class="activity-card">
          <div class="card-header">
            <span class="card-title">
              <uui-icon name="icon-coin-dollar"></uui-icon>
              Store Credit
            </span>
          </div>
          <div class="big-value credit">
            ${this._formatCurrency(this._customer?.storeCreditBalance)}
          </div>
          <div class="value-label">Available Balance</div>

          ${this._showCreditForm ? html`
            <div class="adjustment-form">
              <div class="form-row">
                <div class="form-group">
                  <label>Amount (+/-)</label>
                  <uui-input
                    type="number"
                    step="0.01"
                    .value=${this._creditAmount}
                    @input=${(e) => this._creditAmount = parseFloat(e.target.value) || 0}
                    placeholder="Enter amount"
                  ></uui-input>
                </div>
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label>Reason</label>
                  <uui-input
                    .value=${this._adjustmentReason}
                    @input=${(e) => this._adjustmentReason = e.target.value}
                    placeholder="Enter reason for adjustment"
                  ></uui-input>
                </div>
              </div>
              <div class="action-buttons">
                <uui-button look="secondary" @click=${() => this._showCreditForm = false}>
                  Cancel
                </uui-button>
                <uui-button
                  look="primary"
                  ?disabled=${this._saving || !this._creditAmount}
                  @click=${this._handleAddCredit}
                >
                  ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : 'Apply'}
                </uui-button>
              </div>
            </div>
          ` : html`
            <div class="action-buttons">
              <uui-button look="secondary" @click=${() => this._showCreditForm = true}>
                <uui-icon name="icon-add"></uui-icon>
                Adjust Credit
              </uui-button>
            </div>
          `}
        </div>

        <!-- Customer Info Card -->
        <div class="activity-card">
          <div class="card-header">
            <span class="card-title">
              <uui-icon name="icon-info"></uui-icon>
              Customer Info
            </span>
          </div>
          <ul class="info-list">
            <li>
              <span class="info-label">Customer Since</span>
              <span class="info-value">
                ${this._customer?.createdAt
                  ? new Date(this._customer.createdAt).toLocaleDateString()
                  : 'Unknown'}
              </span>
            </li>
            <li>
              <span class="info-label">Source</span>
              <span class="info-value">${this._customer?.source || 'Direct'}</span>
            </li>
            <li>
              <span class="info-label">Total Orders</span>
              <span class="info-value">${this._customer?.totalOrders || 0}</span>
            </li>
            <li>
              <span class="info-label">Total Spent</span>
              <span class="info-value">${this._formatCurrency(this._customer?.totalSpent)}</span>
            </li>
            <li>
              <span class="info-label">Avg Order Value</span>
              <span class="info-value">${this._formatCurrency(this._customer?.averageOrderValue)}</span>
            </li>
            <li>
              <span class="info-label">Last Order</span>
              <span class="info-value">
                ${this._customer?.lastOrderAt
                  ? new Date(this._customer.lastOrderAt).toLocaleDateString()
                  : 'Never'}
              </span>
            </li>
          </ul>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-customer-activity', CustomerActivity);

export default CustomerActivity;
