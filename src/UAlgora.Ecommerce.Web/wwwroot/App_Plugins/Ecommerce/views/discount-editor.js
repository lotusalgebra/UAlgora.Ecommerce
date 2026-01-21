import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Editor View
 * Form for editing basic discount details.
 */
export class DiscountEditor extends UmbElementMixin(LitElement) {
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

    .form-group uui-input,
    .form-group uui-textarea,
    .form-group uui-select {
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

    .radio-group {
      display: flex;
      gap: var(--uui-size-layout-1);
      flex-wrap: wrap;
    }

    .radio-option {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      min-width: 140px;
    }

    .radio-option:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .radio-option.selected {
      border-color: var(--uui-color-interactive);
      background: var(--uui-color-selected);
    }

    .radio-option input {
      margin: 0;
    }

    .value-input-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .value-input-group uui-input {
      flex: 1;
    }

    .value-suffix {
      font-weight: 500;
      color: var(--uui-color-text-alt);
      min-width: 40px;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
    }

    .buy-x-get-y-group {
      display: grid;
      grid-template-columns: 1fr auto 1fr;
      gap: var(--uui-size-space-4);
      align-items: center;
    }

    .buy-x-get-y-group .separator {
      font-weight: bold;
      color: var(--uui-color-text-alt);
    }

    .code-generate-group {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .code-generate-group uui-input {
      flex: 1;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-2);
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

  _generateCode() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let code = '';
    for (let i = 0; i < 8; i++) {
      code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this._handleInputChange('code', code);
  }

  _getValueSuffix() {
    switch (this._discount?.type) {
      case 'Percentage': return '%';
      case 'FixedAmount': return '$';
      default: return '';
    }
  }

  _showValueInput() {
    return this._discount?.type === 'Percentage' || this._discount?.type === 'FixedAmount';
  }

  _showBuyXGetY() {
    return this._discount?.type === 'BuyXGetY';
  }

  render() {
    if (!this._discount) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <uui-box>
        <h3 class="section-title">Basic Information</h3>
        <div class="form-grid">
          <div class="form-group full-width">
            <label>Discount Name *</label>
            <uui-input
              .value=${this._discount.name || ''}
              @input=${(e) => this._handleInputChange('name', e.target.value)}
              placeholder="e.g., Summer Sale 20% Off"
            ></uui-input>
          </div>
          <div class="form-group full-width">
            <label>Description</label>
            <uui-textarea
              .value=${this._discount.description || ''}
              @input=${(e) => this._handleInputChange('description', e.target.value)}
              placeholder="Internal description for this discount"
              rows="3"
            ></uui-textarea>
          </div>
          <div class="form-group full-width">
            <label>Coupon Code (leave empty for automatic discount)</label>
            <div class="code-generate-group">
              <uui-input
                .value=${this._discount.code || ''}
                @input=${(e) => this._handleInputChange('code', e.target.value.toUpperCase())}
                placeholder="e.g., SUMMER20"
                style="text-transform: uppercase;"
              ></uui-input>
              <uui-button look="secondary" @click=${this._generateCode}>
                Generate
              </uui-button>
            </div>
            <p class="help-text">If empty, discount will be applied automatically when conditions are met.</p>
          </div>
        </div>

        <h3 class="section-title">Discount Type</h3>
        <div class="form-group">
          <div class="radio-group">
            ${['Percentage', 'FixedAmount', 'FreeShipping', 'BuyXGetY'].map(type => html`
              <label
                class="radio-option ${this._discount.type === type ? 'selected' : ''}"
                @click=${() => this._handleInputChange('type', type)}
              >
                <input
                  type="radio"
                  name="discountType"
                  value="${type}"
                  ?checked=${this._discount.type === type}
                />
                ${type === 'Percentage' ? 'Percentage Off' :
                  type === 'FixedAmount' ? 'Fixed Amount Off' :
                  type === 'FreeShipping' ? 'Free Shipping' :
                  'Buy X Get Y'}
              </label>
            `)}
          </div>
        </div>

        ${this._showValueInput() ? html`
          <div class="form-group">
            <label>Discount Value *</label>
            <div class="value-input-group">
              ${this._discount.type === 'FixedAmount' ? html`
                <span class="value-suffix">$</span>
              ` : ''}
              <uui-input
                type="number"
                min="0"
                step="${this._discount.type === 'Percentage' ? '1' : '0.01'}"
                max="${this._discount.type === 'Percentage' ? '100' : ''}"
                .value=${this._discount.value || 0}
                @input=${(e) => this._handleInputChange('value', parseFloat(e.target.value) || 0)}
              ></uui-input>
              ${this._discount.type === 'Percentage' ? html`
                <span class="value-suffix">%</span>
              ` : ''}
            </div>
          </div>

          ${this._discount.type === 'Percentage' ? html`
            <div class="form-group">
              <label>Maximum Discount Amount (optional)</label>
              <div class="value-input-group">
                <span class="value-suffix">$</span>
                <uui-input
                  type="number"
                  min="0"
                  step="0.01"
                  .value=${this._discount.maxDiscountAmount || ''}
                  @input=${(e) => this._handleInputChange('maxDiscountAmount', e.target.value ? parseFloat(e.target.value) : null)}
                  placeholder="No maximum"
                ></uui-input>
              </div>
              <p class="help-text">Caps the discount at this amount regardless of percentage calculation.</p>
            </div>
          ` : ''}
        ` : ''}

        ${this._showBuyXGetY() ? html`
          <div class="form-group">
            <label>Buy X Get Y Configuration</label>
            <div class="buy-x-get-y-group">
              <div>
                <label style="font-size: var(--uui-type-small-size);">Buy Quantity</label>
                <uui-input
                  type="number"
                  min="1"
                  .value=${this._discount.buyQuantity || ''}
                  @input=${(e) => this._handleInputChange('buyQuantity', parseInt(e.target.value) || null)}
                  placeholder="e.g., 2"
                ></uui-input>
              </div>
              <span class="separator">GET</span>
              <div>
                <label style="font-size: var(--uui-type-small-size);">Free Quantity</label>
                <uui-input
                  type="number"
                  min="1"
                  .value=${this._discount.getQuantity || ''}
                  @input=${(e) => this._handleInputChange('getQuantity', parseInt(e.target.value) || null)}
                  placeholder="e.g., 1"
                ></uui-input>
              </div>
            </div>
          </div>
        ` : ''}

        <h3 class="section-title">Scope</h3>
        <div class="form-group">
          <div class="radio-group">
            ${['Order', 'Product', 'Category', 'Shipping'].map(scope => html`
              <label
                class="radio-option ${this._discount.scope === scope ? 'selected' : ''}"
                @click=${() => this._handleInputChange('scope', scope)}
              >
                <input
                  type="radio"
                  name="discountScope"
                  value="${scope}"
                  ?checked=${this._discount.scope === scope}
                />
                ${scope === 'Order' ? 'Entire Order' :
                  scope === 'Product' ? 'Specific Products' :
                  scope === 'Category' ? 'Specific Categories' :
                  'Shipping Only'}
              </label>
            `)}
          </div>
        </div>

        <h3 class="section-title">Status</h3>
        <div class="form-group">
          <label class="checkbox-group">
            <uui-checkbox
              ?checked=${this._discount.isActive}
              @change=${(e) => this._handleInputChange('isActive', e.target.checked)}
            ></uui-checkbox>
            Discount is active
          </label>
        </div>

        <div class="form-grid">
          <div class="form-group">
            <label>Start Date (optional)</label>
            <uui-input
              type="datetime-local"
              .value=${this._discount.startDate ? this._discount.startDate.slice(0, 16) : ''}
              @input=${(e) => this._handleInputChange('startDate', e.target.value ? new Date(e.target.value).toISOString() : null)}
            ></uui-input>
          </div>
          <div class="form-group">
            <label>End Date (optional)</label>
            <uui-input
              type="datetime-local"
              .value=${this._discount.endDate ? this._discount.endDate.slice(0, 16) : ''}
              @input=${(e) => this._handleInputChange('endDate', e.target.value ? new Date(e.target.value).toISOString() : null)}
            ></uui-input>
          </div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-discount-editor', DiscountEditor);

export default DiscountEditor;
