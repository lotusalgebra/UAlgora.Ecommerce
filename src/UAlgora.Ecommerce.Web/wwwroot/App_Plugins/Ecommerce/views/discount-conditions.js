import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Conditions View
 * Form for configuring discount eligibility conditions.
 */
export class DiscountConditions extends UmbElementMixin(LitElement) {
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

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      margin-bottom: var(--uui-size-space-3);
    }

    .value-input-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .value-input-group uui-input {
      flex: 1;
    }

    .value-prefix {
      font-weight: 500;
      color: var(--uui-color-text-alt);
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-2);
    }

    .tags-container {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-space-2);
    }

    .tag {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .tag uui-icon {
      cursor: pointer;
      opacity: 0.6;
    }

    .tag uui-icon:hover {
      opacity: 1;
    }

    .tag-input-group {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .tag-input-group uui-input {
      flex: 1;
    }

    .condition-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .condition-card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-4);
    }

    .condition-card-title {
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _discount: { type: Object, state: true },
    _newTier: { type: String, state: true }
  };

  constructor() {
    super();
    this._discount = null;
    this._newTier = '';
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

  _handleAddTier() {
    if (!this._newTier.trim() || !this._discount) return;

    const tiers = [...(this._discount.eligibleCustomerTiers || []), this._newTier.trim()];
    this._handleInputChange('eligibleCustomerTiers', tiers);
    this._newTier = '';
  }

  _handleRemoveTier(index) {
    if (!this._discount) return;

    const tiers = [...(this._discount.eligibleCustomerTiers || [])];
    tiers.splice(index, 1);
    this._handleInputChange('eligibleCustomerTiers', tiers);
  }

  render() {
    if (!this._discount) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <h3 class="section-title">Order Conditions</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Minimum Order Amount</label>
            <div class="value-input-group">
              <span class="value-prefix">$</span>
              <uui-input
                type="number"
                min="0"
                step="0.01"
                .value=${this._discount.minimumOrderAmount || ''}
                @input=${(e) => this._handleInputChange('minimumOrderAmount', e.target.value ? parseFloat(e.target.value) : null)}
                placeholder="No minimum"
              ></uui-input>
            </div>
            <p class="help-text">Minimum cart subtotal required for discount to apply.</p>
          </div>
          <div class="form-group">
            <label>Minimum Quantity</label>
            <uui-input
              type="number"
              min="0"
              .value=${this._discount.minimumQuantity || ''}
              @input=${(e) => this._handleInputChange('minimumQuantity', e.target.value ? parseInt(e.target.value) : null)}
              placeholder="No minimum"
            ></uui-input>
            <p class="help-text">Minimum number of items in cart.</p>
          </div>
          <div class="form-group">
            <label>Maximum Quantity (items discount applies to)</label>
            <uui-input
              type="number"
              min="0"
              .value=${this._discount.maximumQuantity || ''}
              @input=${(e) => this._handleInputChange('maximumQuantity', e.target.value ? parseInt(e.target.value) : null)}
              placeholder="No maximum"
            ></uui-input>
            <p class="help-text">Limits how many items the discount can be applied to.</p>
          </div>
        </div>
      </uui-box>

      <uui-box>
        <h3 class="section-title">Customer Eligibility</h3>
        <div class="form-group">
          <label class="checkbox-group">
            <uui-checkbox
              ?checked=${this._discount.firstTimeCustomerOnly}
              @change=${(e) => this._handleInputChange('firstTimeCustomerOnly', e.target.checked)}
            ></uui-checkbox>
            First-time customers only
          </label>
          <p class="help-text">Only customers who have never placed an order can use this discount.</p>
        </div>

        <div class="form-group">
          <label>Eligible Customer Tiers</label>
          <div class="tags-container">
            ${(this._discount.eligibleCustomerTiers || []).map((tier, index) => html`
              <span class="tag">
                ${tier}
                <uui-icon
                  name="icon-delete"
                  @click=${() => this._handleRemoveTier(index)}
                ></uui-icon>
              </span>
            `)}
          </div>
          <div class="tag-input-group">
            <uui-input
              .value=${this._newTier}
              @input=${(e) => this._newTier = e.target.value}
              @keypress=${(e) => e.key === 'Enter' && this._handleAddTier()}
              placeholder="e.g., Gold, VIP, Silver"
            ></uui-input>
            <uui-button look="secondary" @click=${this._handleAddTier}>
              Add Tier
            </uui-button>
          </div>
          <p class="help-text">Leave empty for all customers. Add tiers to restrict to specific customer segments.</p>
        </div>
      </uui-box>

      <uui-box>
        <h3 class="section-title">Product Exclusions</h3>
        <div class="form-group">
          <label class="checkbox-group">
            <uui-checkbox
              ?checked=${this._discount.excludeSaleItems}
              @change=${(e) => this._handleInputChange('excludeSaleItems', e.target.checked)}
            ></uui-checkbox>
            Exclude sale items
          </label>
          <p class="help-text">Products already on sale will not receive additional discount.</p>
        </div>
      </uui-box>

      <uui-box>
        <h3 class="section-title">Stacking Rules</h3>
        <div class="form-group">
          <label class="checkbox-group">
            <uui-checkbox
              ?checked=${this._discount.canCombine}
              @change=${(e) => this._handleInputChange('canCombine', e.target.checked)}
            ></uui-checkbox>
            Can be combined with other discounts
          </label>
          <p class="help-text">When enabled, this discount can be used alongside other discounts.</p>
        </div>

        <div class="form-group">
          <label>Priority</label>
          <uui-input
            type="number"
            min="0"
            .value=${this._discount.priority || 0}
            @input=${(e) => this._handleInputChange('priority', parseInt(e.target.value) || 0)}
          ></uui-input>
          <p class="help-text">Higher priority discounts are applied first. Default is 0.</p>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-discount-conditions', DiscountConditions);

export default DiscountConditions;
