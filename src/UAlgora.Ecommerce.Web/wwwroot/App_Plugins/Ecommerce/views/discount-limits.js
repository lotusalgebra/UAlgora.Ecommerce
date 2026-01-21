import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Limits View
 * Form for configuring discount usage limits.
 */
export class DiscountLimits extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    .form-group uui-input {
      width: 100%;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-3);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .section-title:first-child {
      margin-top: 0;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-2);
    }

    .stats-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .stat-card {
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      text-align: center;
    }

    .stat-value {
      font-size: var(--uui-type-h2-size);
      font-weight: bold;
      color: var(--uui-color-interactive);
      margin-bottom: var(--uui-size-space-2);
    }

    .stat-label {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .progress-bar {
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
      height: 8px;
      overflow: hidden;
      margin-top: var(--uui-size-space-2);
    }

    .progress-fill {
      background: var(--uui-color-positive);
      height: 100%;
      transition: width 0.3s ease;
    }

    .progress-fill.warning {
      background: var(--uui-color-warning);
    }

    .progress-fill.danger {
      background: var(--uui-color-danger);
    }

    .limit-status {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-layout-1);
    }

    .limit-status.reached {
      border-color: var(--uui-color-danger);
      background: var(--uui-color-danger-emphasis);
    }

    .limit-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .limit-icon {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: var(--uui-color-surface-alt);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .limit-icon.reached {
      background: var(--uui-color-danger);
      color: #fff;
    }
  `;

  static properties = {
    _discount: { type: Object, state: true }
  };

  constructor() {
    super();
    this._discount = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-discount-workspace');
    if (workspace) {
      this._discount = workspace.getDiscount();
    }
  }

  _handleInputChange(field, value) {
    if (!this._discount) return;

    this._discount = {
      ...this._discount,
      [field]: value
    };

    const workspace = this.closest('ecommerce-discount-workspace');
    if (workspace) {
      workspace.setDiscount(this._discount);
    }
  }

  _getUsagePercentage() {
    if (!this._discount?.totalUsageLimit) return 0;
    return Math.min(100, (this._discount.usageCount / this._discount.totalUsageLimit) * 100);
  }

  _getProgressClass() {
    const percentage = this._getUsagePercentage();
    if (percentage >= 100) return 'danger';
    if (percentage >= 80) return 'warning';
    return '';
  }

  _getRemainingUses() {
    if (!this._discount?.totalUsageLimit) return 'Unlimited';
    const remaining = this._discount.totalUsageLimit - this._discount.usageCount;
    return remaining > 0 ? remaining : 0;
  }

  render() {
    if (!this._discount) {
      return html`<uui-loader></uui-loader>`;
    }

    const isLimitReached = this._discount.totalUsageLimit && this._discount.usageCount >= this._discount.totalUsageLimit;

    return html`
      <uui-box>
        <h3 class="section-title">Current Usage</h3>

        ${this._discount.totalUsageLimit ? html`
          <div class="limit-status ${isLimitReached ? 'reached' : ''}">
            <div class="limit-info">
              <div class="limit-icon ${isLimitReached ? 'reached' : ''}">
                <uui-icon name="${isLimitReached ? 'icon-block' : 'icon-check'}"></uui-icon>
              </div>
              <div>
                <strong>${isLimitReached ? 'Usage Limit Reached' : 'Usage Limit Active'}</strong>
                <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">
                  ${this._discount.usageCount} of ${this._discount.totalUsageLimit} uses
                </div>
              </div>
            </div>
            <div style="text-align: right;">
              <strong>${this._getRemainingUses()}</strong>
              <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">
                remaining
              </div>
            </div>
          </div>
          <div class="progress-bar">
            <div
              class="progress-fill ${this._getProgressClass()}"
              style="width: ${this._getUsagePercentage()}%"
            ></div>
          </div>
        ` : html`
          <div class="stats-row">
            <div class="stat-card">
              <div class="stat-value">${this._discount.usageCount || 0}</div>
              <div class="stat-label">Total Uses</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">Unlimited</div>
              <div class="stat-label">No Limit Set</div>
            </div>
          </div>
        `}
      </uui-box>

      <uui-box>
        <h3 class="section-title">Usage Limits</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Total Usage Limit</label>
            <uui-input
              type="number"
              min="0"
              .value=${this._discount.totalUsageLimit || ''}
              @input=${(e) => this._handleInputChange('totalUsageLimit', e.target.value ? parseInt(e.target.value) : null)}
              placeholder="Unlimited"
            ></uui-input>
            <p class="help-text">Maximum number of times this discount can be used across all customers.</p>
          </div>
          <div class="form-group">
            <label>Per Customer Limit</label>
            <uui-input
              type="number"
              min="0"
              .value=${this._discount.perCustomerLimit || ''}
              @input=${(e) => this._handleInputChange('perCustomerLimit', e.target.value ? parseInt(e.target.value) : null)}
              placeholder="Unlimited"
            ></uui-input>
            <p class="help-text">Maximum times a single customer can use this discount.</p>
          </div>
        </div>
      </uui-box>

      <uui-box>
        <h3 class="section-title">Validity Period</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Start Date</label>
            <uui-input
              type="datetime-local"
              .value=${this._discount.startDate ? this._discount.startDate.slice(0, 16) : ''}
              @input=${(e) => this._handleInputChange('startDate', e.target.value ? new Date(e.target.value).toISOString() : null)}
            ></uui-input>
            <p class="help-text">When the discount becomes active. Leave empty for immediate activation.</p>
          </div>
          <div class="form-group">
            <label>End Date</label>
            <uui-input
              type="datetime-local"
              .value=${this._discount.endDate ? this._discount.endDate.slice(0, 16) : ''}
              @input=${(e) => this._handleInputChange('endDate', e.target.value ? new Date(e.target.value).toISOString() : null)}
            ></uui-input>
            <p class="help-text">When the discount expires. Leave empty for no expiration.</p>
          </div>
        </div>

        ${this._discount.startDate || this._discount.endDate ? html`
          <div style="margin-top: var(--uui-size-layout-1); padding: var(--uui-size-space-4); background: var(--uui-color-surface-alt); border-radius: var(--uui-border-radius);">
            <strong>Validity Summary:</strong>
            <span style="color: var(--uui-color-text-alt);">
              ${this._discount.startDate
                ? `From ${new Date(this._discount.startDate).toLocaleString()}`
                : 'Immediately'}
              ${this._discount.endDate
                ? ` until ${new Date(this._discount.endDate).toLocaleString()}`
                : ' (no expiration)'}
            </span>
          </div>
        ` : ''}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-discount-limits', DiscountLimits);

export default DiscountLimits;
