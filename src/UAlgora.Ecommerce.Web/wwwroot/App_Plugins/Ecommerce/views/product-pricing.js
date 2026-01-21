import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Pricing View
 * Manages advanced pricing options including tiered pricing, sale scheduling, and compare at price.
 */
export class ProductPricing extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--uui-size-layout-1);
    }

    .form-grid.three-col {
      grid-template-columns: 1fr 1fr 1fr;
    }

    .form-group {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    uui-input, uui-select {
      width: 100%;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-2);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .price-summary {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .price-card {
      background: var(--uui-color-surface-alt);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
      text-align: center;
    }

    .price-card-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-2);
    }

    .price-card-value {
      font-size: var(--uui-type-h3-size);
      font-weight: bold;
    }

    .price-card-value.positive {
      color: var(--uui-color-positive);
    }

    .price-card-value.warning {
      color: var(--uui-color-warning);
    }

    .tiered-pricing-table {
      width: 100%;
      border-collapse: collapse;
    }

    .tiered-pricing-table th,
    .tiered-pricing-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .tiered-pricing-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .tier-row {
      display: flex;
      gap: var(--uui-size-space-2);
      align-items: center;
    }

    .tier-actions {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-2);
      color: var(--uui-color-text-alt);
    }

    .sale-status {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-layout-1);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-layout-1);
    }

    .sale-status.active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive-standalone);
    }

    .sale-status.scheduled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning-standalone);
    }

    .sale-status.inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .profit-indicator {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .profit-indicator.positive {
      color: var(--uui-color-positive);
    }

    .profit-indicator.negative {
      color: var(--uui-color-danger);
    }
  `;

  static properties = {
    _product: { type: Object, state: true },
    _priceTiers: { type: Array, state: true }
  };

  constructor() {
    super();
    this._product = null;
    this._priceTiers = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      this._product = workspace.getProduct();
      this._priceTiers = this._product?.priceTiers || [];
    }
  }

  _handleNumberInput(field, event) {
    const value = event.target.value ? parseFloat(event.target.value) : null;
    this._product = { ...this._product, [field]: value };
    this._updateWorkspace();
  }

  _handleDateInput(field, event) {
    const value = event.target.value || null;
    this._product = { ...this._product, [field]: value };
    this._updateWorkspace();
  }

  _handleAddTier() {
    const newTier = {
      id: crypto.randomUUID(),
      minQuantity: this._priceTiers.length > 0
        ? Math.max(...this._priceTiers.map(t => t.minQuantity)) + 10
        : 10,
      price: this._product?.basePrice || 0,
      discountPercent: 5
    };

    this._priceTiers = [...this._priceTiers, newTier];
    this._updateWorkspace();
  }

  _handleTierInput(tierId, field, value) {
    this._priceTiers = this._priceTiers.map(t =>
      t.id === tierId ? { ...t, [field]: value } : t
    );
    this._updateWorkspace();
  }

  _handleDeleteTier(tierId) {
    this._priceTiers = this._priceTiers.filter(t => t.id !== tierId);
    this._updateWorkspace();
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      workspace.setProduct({
        ...this._product,
        priceTiers: this._priceTiers
      });
    }
  }

  _getEffectivePrice() {
    const now = new Date();
    const saleStart = this._product?.saleStartDate ? new Date(this._product.saleStartDate) : null;
    const saleEnd = this._product?.saleEndDate ? new Date(this._product.saleEndDate) : null;

    if (this._product?.salePrice) {
      if ((!saleStart || now >= saleStart) && (!saleEnd || now <= saleEnd)) {
        return this._product.salePrice;
      }
    }

    return this._product?.basePrice || 0;
  }

  _getSaleStatus() {
    if (!this._product?.salePrice) {
      return { status: 'inactive', message: 'No sale configured' };
    }

    const now = new Date();
    const saleStart = this._product?.saleStartDate ? new Date(this._product.saleStartDate) : null;
    const saleEnd = this._product?.saleEndDate ? new Date(this._product.saleEndDate) : null;

    if (saleStart && now < saleStart) {
      return { status: 'scheduled', message: `Sale starts ${saleStart.toLocaleDateString()}` };
    }

    if (saleEnd && now > saleEnd) {
      return { status: 'inactive', message: `Sale ended ${saleEnd.toLocaleDateString()}` };
    }

    return { status: 'active', message: 'Sale is currently active' };
  }

  _getProfit() {
    const effectivePrice = this._getEffectivePrice();
    const costPrice = this._product?.costPrice || 0;
    return effectivePrice - costPrice;
  }

  _getProfitMargin() {
    const effectivePrice = this._getEffectivePrice();
    const costPrice = this._product?.costPrice || 0;
    if (effectivePrice === 0) return 0;
    return ((effectivePrice - costPrice) / effectivePrice) * 100;
  }

  render() {
    if (!this._product) {
      return html`<uui-loader></uui-loader>`;
    }

    const saleStatus = this._getSaleStatus();
    const profit = this._getProfit();
    const profitMargin = this._getProfitMargin();
    const effectivePrice = this._getEffectivePrice();

    return html`
      <div class="price-summary">
        <div class="price-card">
          <div class="price-card-label">Effective Price</div>
          <div class="price-card-value">$${effectivePrice.toFixed(2)}</div>
        </div>
        <div class="price-card">
          <div class="price-card-label">Profit per Unit</div>
          <div class="price-card-value ${profit >= 0 ? 'positive' : 'warning'}">
            ${profit >= 0 ? '+' : ''}$${profit.toFixed(2)}
          </div>
        </div>
        <div class="price-card">
          <div class="price-card-label">Profit Margin</div>
          <div class="price-card-value ${profitMargin >= 20 ? 'positive' : 'warning'}">
            ${profitMargin.toFixed(1)}%
          </div>
        </div>
      </div>

      <uui-box>
        <div slot="headline">Base Pricing</div>

        <div class="form-grid three-col">
          <div class="form-group">
            <label for="basePrice">Base Price *</label>
            <uui-input
              id="basePrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.basePrice?.toString() || '0'}
              @input=${(e) => this._handleNumberInput('basePrice', e)}
            ></uui-input>
            <div class="hint">Regular selling price</div>
          </div>

          <div class="form-group">
            <label for="costPrice">Cost Price</label>
            <uui-input
              id="costPrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.costPrice?.toString() || ''}
              @input=${(e) => this._handleNumberInput('costPrice', e)}
              placeholder="Your cost"
            ></uui-input>
            <div class="hint">Used for profit calculation</div>
          </div>

          <div class="form-group">
            <label for="compareAtPrice">Compare at Price</label>
            <uui-input
              id="compareAtPrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.compareAtPrice?.toString() || ''}
              @input=${(e) => this._handleNumberInput('compareAtPrice', e)}
              placeholder="Original price"
            ></uui-input>
            <div class="hint">Shows as strikethrough price</div>
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Sale Pricing</h3>
      <uui-box>
        <div class="sale-status ${saleStatus.status}">
          <uui-icon name="${saleStatus.status === 'active' ? 'icon-check' : saleStatus.status === 'scheduled' ? 'icon-calendar' : 'icon-remove'}"></uui-icon>
          <span>${saleStatus.message}</span>
        </div>

        <div class="form-grid three-col">
          <div class="form-group">
            <label for="salePrice">Sale Price</label>
            <uui-input
              id="salePrice"
              type="number"
              step="0.01"
              min="0"
              .value=${this._product.salePrice?.toString() || ''}
              @input=${(e) => this._handleNumberInput('salePrice', e)}
              placeholder="Leave empty for no sale"
            ></uui-input>
            <div class="hint">Discounted price when on sale</div>
          </div>

          <div class="form-group">
            <label for="saleStartDate">Sale Start Date</label>
            <uui-input
              id="saleStartDate"
              type="datetime-local"
              .value=${this._product.saleStartDate || ''}
              @input=${(e) => this._handleDateInput('saleStartDate', e)}
            ></uui-input>
            <div class="hint">Leave empty to start immediately</div>
          </div>

          <div class="form-group">
            <label for="saleEndDate">Sale End Date</label>
            <uui-input
              id="saleEndDate"
              type="datetime-local"
              .value=${this._product.saleEndDate || ''}
              @input=${(e) => this._handleDateInput('saleEndDate', e)}
            ></uui-input>
            <div class="hint">Leave empty for no end date</div>
          </div>
        </div>
      </uui-box>

      <h3 class="section-title">Tiered Pricing (Quantity Discounts)</h3>
      <uui-box>
        <p style="color: var(--uui-color-text-alt); margin-bottom: var(--uui-size-layout-1);">
          Offer discounts when customers buy in larger quantities.
        </p>

        ${this._priceTiers.length > 0 ? html`
          <table class="tiered-pricing-table">
            <thead>
              <tr>
                <th>Minimum Quantity</th>
                <th>Price per Unit</th>
                <th>Discount %</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              ${this._priceTiers.map(tier => html`
                <tr>
                  <td>
                    <uui-input
                      type="number"
                      min="1"
                      .value=${tier.minQuantity?.toString() || '1'}
                      @input=${(e) => this._handleTierInput(tier.id, 'minQuantity', parseInt(e.target.value) || 1)}
                      style="width: 100px"
                    ></uui-input>
                  </td>
                  <td>
                    <uui-input
                      type="number"
                      step="0.01"
                      min="0"
                      .value=${tier.price?.toString() || '0'}
                      @input=${(e) => this._handleTierInput(tier.id, 'price', parseFloat(e.target.value) || 0)}
                      style="width: 120px"
                    ></uui-input>
                  </td>
                  <td>
                    <div class="profit-indicator ${((this._product.basePrice - tier.price) / this._product.basePrice * 100) > 0 ? 'positive' : ''}">
                      ${this._product.basePrice > 0
                        ? ((this._product.basePrice - tier.price) / this._product.basePrice * 100).toFixed(1)
                        : 0}%
                    </div>
                  </td>
                  <td>
                    <div class="tier-actions">
                      <uui-button
                        look="secondary"
                        color="danger"
                        compact
                        @click=${() => this._handleDeleteTier(tier.id)}
                      >
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </div>
                  </td>
                </tr>
              `)}
            </tbody>
          </table>
        ` : html`
          <div class="empty-state">
            <p>No tiered pricing configured</p>
          </div>
        `}

        <div style="margin-top: var(--uui-size-layout-1)">
          <uui-button look="primary" @click=${this._handleAddTier}>
            <uui-icon name="icon-add"></uui-icon>
            Add Price Tier
          </uui-button>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-product-pricing', ProductPricing);

export default ProductPricing;
