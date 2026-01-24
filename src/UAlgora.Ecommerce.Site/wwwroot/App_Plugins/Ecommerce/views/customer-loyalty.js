import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Loyalty View
 * Manages loyalty points, store credit, and customer tier.
 */
export class CustomerLoyalty extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .loyalty-container {
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-layout-1);
    }

    .summary-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: var(--uui-size-layout-1);
    }

    .summary-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-4);
    }

    .card-icon {
      width: 48px;
      height: 48px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 24px;
    }

    .icon-points {
      background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
      color: #fff;
    }

    .icon-credit {
      background: linear-gradient(135deg, #4caf50 0%, #2e7d32 100%);
      color: #fff;
    }

    .icon-tier {
      background: linear-gradient(135deg, #9c27b0 0%, #673ab7 100%);
      color: #fff;
    }

    .card-value {
      font-size: var(--uui-type-h2-size);
      font-weight: bold;
      margin-bottom: var(--uui-size-space-2);
    }

    .card-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
      margin-bottom: var(--uui-size-space-4);
    }

    .card-actions {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .tier-badge {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      border-radius: var(--uui-border-radius);
      font-weight: bold;
    }

    .tier-standard { background: #e0e0e0; color: #424242; }
    .tier-bronze { background: #cd7f32; color: #fff; }
    .tier-silver { background: #c0c0c0; color: #000; }
    .tier-gold { background: #ffd700; color: #000; }
    .tier-platinum { background: #e5e4e2; color: #000; }
    .tier-vip { background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); color: #fff; }

    .adjustment-form {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      margin-top: var(--uui-size-layout-1);
    }

    .form-title {
      font-size: var(--uui-type-h5-size);
      font-weight: bold;
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 2fr;
      gap: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-space-4);
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
      margin-top: var(--uui-size-space-4);
    }

    .type-options {
      display: flex;
      gap: var(--uui-size-space-4);
    }

    .type-option {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      cursor: pointer;
    }

    .lifetime-stats {
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .lifetime-title {
      font-weight: bold;
      margin-bottom: var(--uui-size-space-4);
    }

    .lifetime-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: var(--uui-size-space-4);
    }

    .lifetime-item {
      text-align: center;
    }

    .lifetime-value {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      color: var(--uui-color-interactive);
    }

    .lifetime-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .tier-selector {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .tier-options {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-3);
      margin-top: var(--uui-size-space-4);
    }

    .tier-option {
      padding: var(--uui-size-space-3) var(--uui-size-space-5);
      border: 2px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      transition: all 0.2s;
    }

    .tier-option:hover {
      border-color: var(--uui-color-interactive);
    }

    .tier-option.selected {
      border-color: var(--uui-color-positive);
      background: var(--uui-color-positive-emphasis);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _customer: { type: Object, state: true },
    _showPointsForm: { type: Boolean, state: true },
    _showCreditForm: { type: Boolean, state: true },
    _pointsAmount: { type: Number, state: true },
    _pointsType: { type: String, state: true },
    _pointsReason: { type: String, state: true },
    _creditAmount: { type: Number, state: true },
    _creditType: { type: String, state: true },
    _creditReason: { type: String, state: true },
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._customer = null;
    this._showPointsForm = false;
    this._showCreditForm = false;
    this._pointsAmount = 0;
    this._pointsType = 'add';
    this._pointsReason = '';
    this._creditAmount = 0;
    this._creditType = 'add';
    this._creditReason = '';
    this._processing = false;
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

  _formatPoints(points) {
    return new Intl.NumberFormat('en-US').format(points || 0);
  }

  _getTierClass() {
    const tier = this._customer?.customerTier?.toLowerCase() || 'standard';
    return `tier-${tier}`;
  }

  _togglePointsForm() {
    this._showPointsForm = !this._showPointsForm;
    if (this._showPointsForm) {
      this._pointsAmount = 0;
      this._pointsType = 'add';
      this._pointsReason = '';
    }
  }

  _toggleCreditForm() {
    this._showCreditForm = !this._showCreditForm;
    if (this._showCreditForm) {
      this._creditAmount = 0;
      this._creditType = 'add';
      this._creditReason = '';
    }
  }

  async _adjustLoyaltyPoints() {
    if (!this._customer?.id || !this._pointsAmount) {
      alert('Please enter a valid points amount');
      return;
    }

    const points = this._pointsType === 'add' ? this._pointsAmount : -this._pointsAmount;

    if (this._pointsType === 'deduct' && Math.abs(points) > this._customer.loyaltyPoints) {
      alert('Cannot deduct more points than the customer has');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${this._customer.id}/loyalty-points`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          points: points,
          reason: this._pointsReason
        })
      });

      if (response.ok) {
        const result = await response.json();
        const workspace = this.closest('ecommerce-customer-workspace');
        if (workspace) {
          workspace.setCustomer(result);
          this._customer = result;
        }
        this._showPointsForm = false;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to adjust loyalty points');
      }
    } catch (error) {
      console.error('Error adjusting loyalty points:', error);
      alert('Failed to adjust loyalty points');
    } finally {
      this._processing = false;
    }
  }

  async _adjustStoreCredit() {
    if (!this._customer?.id || !this._creditAmount) {
      alert('Please enter a valid credit amount');
      return;
    }

    const amount = this._creditType === 'add' ? this._creditAmount : -this._creditAmount;

    if (this._creditType === 'deduct' && Math.abs(amount) > this._customer.storeCreditBalance) {
      alert('Cannot deduct more credit than the customer has');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${this._customer.id}/store-credit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          amount: amount,
          reason: this._creditReason
        })
      });

      if (response.ok) {
        const result = await response.json();
        const workspace = this.closest('ecommerce-customer-workspace');
        if (workspace) {
          workspace.setCustomer(result);
          this._customer = result;
        }
        this._showCreditForm = false;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to adjust store credit');
      }
    } catch (error) {
      console.error('Error adjusting store credit:', error);
      alert('Failed to adjust store credit');
    } finally {
      this._processing = false;
    }
  }

  async _updateTier(tier) {
    if (!this._customer?.id) return;

    this._processing = true;

    try {
      const workspace = this.closest('ecommerce-customer-workspace');
      if (workspace) {
        const customer = workspace.getCustomer();
        customer.customerTier = tier;
        workspace.setCustomer(customer);
        this._customer = customer;
      }
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._customer) {
      return html`<uui-loader></uui-loader>`;
    }

    const isNew = !this._customer.id;

    if (isNew) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-medal" style="font-size: 48px;"></uui-icon>
          <h3>Loyalty & Credits</h3>
          <p>Save the customer first to manage loyalty points and store credit.</p>
        </div>
      `;
    }

    return html`
      <div class="loyalty-container">
        <div class="summary-cards">
          ${this._renderPointsCard()}
          ${this._renderCreditCard()}
          ${this._renderTierCard()}
        </div>

        ${this._showPointsForm ? this._renderPointsForm() : ''}
        ${this._showCreditForm ? this._renderCreditForm() : ''}

        ${this._renderLifetimeStats()}
        ${this._renderTierSelector()}
      </div>
    `;
  }

  _renderPointsCard() {
    return html`
      <div class="summary-card">
        <div class="card-header">
          <div>
            <div class="card-value">${this._formatPoints(this._customer.loyaltyPoints)}</div>
            <div class="card-label">Loyalty Points Balance</div>
          </div>
          <div class="card-icon icon-points">
            <uui-icon name="icon-medal"></uui-icon>
          </div>
        </div>
        <div class="card-actions">
          <uui-button look="primary" compact @click=${this._togglePointsForm}>
            ${this._showPointsForm ? 'Cancel' : 'Adjust Points'}
          </uui-button>
        </div>
      </div>
    `;
  }

  _renderCreditCard() {
    return html`
      <div class="summary-card">
        <div class="card-header">
          <div>
            <div class="card-value">${this._formatCurrency(this._customer.storeCreditBalance)}</div>
            <div class="card-label">Store Credit Balance</div>
          </div>
          <div class="card-icon icon-credit">
            <uui-icon name="icon-coin-dollar"></uui-icon>
          </div>
        </div>
        <div class="card-actions">
          <uui-button look="primary" compact @click=${this._toggleCreditForm}>
            ${this._showCreditForm ? 'Cancel' : 'Adjust Credit'}
          </uui-button>
        </div>
      </div>
    `;
  }

  _renderTierCard() {
    const tier = this._customer.customerTier || 'Standard';

    return html`
      <div class="summary-card">
        <div class="card-header">
          <div>
            <div class="card-value">
              <span class="tier-badge ${this._getTierClass()}">
                <uui-icon name="icon-crown"></uui-icon>
                ${tier}
              </span>
            </div>
            <div class="card-label">Customer Tier</div>
          </div>
          <div class="card-icon icon-tier">
            <uui-icon name="icon-users"></uui-icon>
          </div>
        </div>
      </div>
    `;
  }

  _renderPointsForm() {
    return html`
      <div class="adjustment-form">
        <h4 class="form-title">Adjust Loyalty Points</h4>

        <div class="form-group">
          <label>Type</label>
          <div class="type-options">
            <label class="type-option">
              <input
                type="radio"
                name="pointsType"
                value="add"
                ?checked=${this._pointsType === 'add'}
                @change=${() => this._pointsType = 'add'}
              />
              Add Points
            </label>
            <label class="type-option">
              <input
                type="radio"
                name="pointsType"
                value="deduct"
                ?checked=${this._pointsType === 'deduct'}
                @change=${() => this._pointsType = 'deduct'}
              />
              Deduct Points
            </label>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Points</label>
            <uui-input
              type="number"
              min="1"
              .value=${this._pointsAmount}
              @input=${(e) => this._pointsAmount = parseInt(e.target.value) || 0}
              placeholder="Enter points amount"
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Reason</label>
            <uui-input
              .value=${this._pointsReason}
              @input=${(e) => this._pointsReason = e.target.value}
              placeholder="e.g., Manual adjustment, Compensation, Promotion"
            ></uui-input>
          </div>
        </div>

        <div class="form-actions">
          <uui-button look="secondary" @click=${this._togglePointsForm}>
            Cancel
          </uui-button>
          <uui-button
            look="primary"
            color="${this._pointsType === 'add' ? 'positive' : 'warning'}"
            ?disabled=${this._processing || !this._pointsAmount}
            @click=${this._adjustLoyaltyPoints}
          >
            ${this._processing ? 'Processing...' : `${this._pointsType === 'add' ? 'Add' : 'Deduct'} ${this._formatPoints(this._pointsAmount)} Points`}
          </uui-button>
        </div>
      </div>
    `;
  }

  _renderCreditForm() {
    return html`
      <div class="adjustment-form">
        <h4 class="form-title">Adjust Store Credit</h4>

        <div class="form-group">
          <label>Type</label>
          <div class="type-options">
            <label class="type-option">
              <input
                type="radio"
                name="creditType"
                value="add"
                ?checked=${this._creditType === 'add'}
                @change=${() => this._creditType = 'add'}
              />
              Add Credit
            </label>
            <label class="type-option">
              <input
                type="radio"
                name="creditType"
                value="deduct"
                ?checked=${this._creditType === 'deduct'}
                @change=${() => this._creditType = 'deduct'}
              />
              Deduct Credit
            </label>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Amount</label>
            <uui-input
              type="number"
              min="0.01"
              step="0.01"
              .value=${this._creditAmount}
              @input=${(e) => this._creditAmount = parseFloat(e.target.value) || 0}
              placeholder="Enter credit amount"
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Reason</label>
            <uui-input
              .value=${this._creditReason}
              @input=${(e) => this._creditReason = e.target.value}
              placeholder="e.g., Refund, Compensation, Gift card"
            ></uui-input>
          </div>
        </div>

        <div class="form-actions">
          <uui-button look="secondary" @click=${this._toggleCreditForm}>
            Cancel
          </uui-button>
          <uui-button
            look="primary"
            color="${this._creditType === 'add' ? 'positive' : 'warning'}"
            ?disabled=${this._processing || !this._creditAmount}
            @click=${this._adjustStoreCredit}
          >
            ${this._processing ? 'Processing...' : `${this._creditType === 'add' ? 'Add' : 'Deduct'} ${this._formatCurrency(this._creditAmount)}`}
          </uui-button>
        </div>
      </div>
    `;
  }

  _renderLifetimeStats() {
    return html`
      <div class="lifetime-stats">
        <h4 class="lifetime-title">Lifetime Statistics</h4>
        <div class="lifetime-grid">
          <div class="lifetime-item">
            <div class="lifetime-value">${this._formatPoints(this._customer.totalLoyaltyPointsEarned || 0)}</div>
            <div class="lifetime-label">Total Points Earned</div>
          </div>
          <div class="lifetime-item">
            <div class="lifetime-value">${this._formatCurrency(this._customer.totalSpent)}</div>
            <div class="lifetime-label">Total Spent</div>
          </div>
          <div class="lifetime-item">
            <div class="lifetime-value">${this._customer.totalOrders || 0}</div>
            <div class="lifetime-label">Total Orders</div>
          </div>
          <div class="lifetime-item">
            <div class="lifetime-value">${this._formatCurrency(this._customer.averageOrderValue)}</div>
            <div class="lifetime-label">Avg Order Value</div>
          </div>
        </div>
      </div>
    `;
  }

  _renderTierSelector() {
    const tiers = ['Standard', 'Bronze', 'Silver', 'Gold', 'Platinum', 'VIP'];
    const currentTier = this._customer.customerTier || 'Standard';

    return html`
      <div class="tier-selector">
        <h4 class="lifetime-title">Update Customer Tier</h4>
        <p style="color: var(--uui-color-text-alt); margin-bottom: var(--uui-size-space-4);">
          Select a tier to assign to this customer. Higher tiers typically receive better benefits.
        </p>
        <div class="tier-options">
          ${tiers.map(tier => html`
            <div
              class="tier-option ${currentTier === tier ? 'selected' : ''}"
              @click=${() => this._updateTier(tier)}
            >
              <span class="tier-badge tier-${tier.toLowerCase()}">${tier}</span>
            </div>
          `)}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-customer-loyalty', CustomerLoyalty);

export default CustomerLoyalty;
