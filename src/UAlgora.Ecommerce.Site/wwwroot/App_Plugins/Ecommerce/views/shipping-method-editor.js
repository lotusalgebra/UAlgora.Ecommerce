import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Editor
 * Form for editing shipping method details and calculation settings.
 */
export class ShippingMethodEditor extends UmbElementMixin(LitElement) {
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
      display: flex;
      flex-direction: column;
      gap: var(--uui-size-space-2);
    }

    .form-group.full-width {
      grid-column: 1 / -1;
    }

    .form-group label {
      font-weight: 500;
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .section-title {
      font-size: var(--uui-type-h5-size);
      font-weight: bold;
      margin-bottom: var(--uui-size-space-4);
    }

    .type-selector {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--uui-size-space-4);
    }

    .type-option {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: var(--uui-size-space-5);
      border: 2px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      transition: all 0.2s;
    }

    .type-option:hover {
      border-color: var(--uui-color-interactive);
      background: var(--uui-color-surface-emphasis);
    }

    .type-option.selected {
      border-color: var(--uui-color-selected);
      background: var(--uui-color-selected-emphasis);
    }

    .type-option uui-icon {
      font-size: 32px;
      margin-bottom: var(--uui-size-space-2);
    }

    .type-option .type-name {
      font-weight: 500;
      margin-bottom: var(--uui-size-space-1);
    }

    .type-option .type-desc {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      text-align: center;
    }

    .rate-config {
      background: var(--uui-color-surface-alt);
      padding: var(--uui-size-layout-1);
      border-radius: var(--uui-border-radius);
    }

    .delivery-estimate {
      display: flex;
      gap: var(--uui-size-space-4);
      align-items: center;
    }

    .delivery-estimate span {
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _method: { type: Object, state: true }
  };

  constructor() {
    super();
    this._method = {};
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-shipping-method-workspace');
    if (workspace) {
      this._method = workspace.getMethod();
    }
  }

  _updateMethod(field, value) {
    const workspace = this.closest('ecommerce-shipping-method-workspace');
    if (workspace) {
      workspace.setMethod({ [field]: value });
      this._method = workspace.getMethod();
    }
  }

  _handleTypeSelect(type) {
    this._updateMethod('calculationType', type);
  }

  render() {
    return html`
      <uui-box headline="Basic Information">
        <div class="form-grid">
          <div class="form-group">
            <label>Method Name *</label>
            <uui-input
              placeholder="e.g., Standard Shipping"
              .value=${this._method.name || ''}
              @input=${(e) => this._updateMethod('name', e.target.value)}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Code *</label>
            <uui-input
              placeholder="e.g., standard"
              .value=${this._method.code || ''}
              @input=${(e) => this._updateMethod('code', e.target.value)}
            ></uui-input>
            <span class="hint">Unique identifier for this method</span>
          </div>

          <div class="form-group full-width">
            <label>Description</label>
            <uui-textarea
              placeholder="Describe this shipping method..."
              .value=${this._method.description || ''}
              @input=${(e) => this._updateMethod('description', e.target.value)}
            ></uui-textarea>
          </div>

          <div class="form-group">
            <label>Sort Order</label>
            <uui-input
              type="number"
              .value=${this._method.sortOrder?.toString() || '0'}
              @input=${(e) => this._updateMethod('sortOrder', parseInt(e.target.value) || 0)}
            ></uui-input>
          </div>

          <div class="form-group">
            <uui-toggle
              ?checked=${this._method.isActive}
              @change=${(e) => this._updateMethod('isActive', e.target.checked)}
            >
              Active
            </uui-toggle>
          </div>
        </div>
      </uui-box>

      <uui-box headline="Calculation Type">
        <div class="type-selector">
          ${this._renderTypeOption('FlatRate', 'icon-coin', 'Flat Rate', 'Fixed shipping cost')}
          ${this._renderTypeOption('FreeShipping', 'icon-gift', 'Free Shipping', 'No shipping charge')}
          ${this._renderTypeOption('WeightBased', 'icon-scale', 'Weight Based', 'Based on order weight')}
          ${this._renderTypeOption('PriceBased', 'icon-coins-dollar', 'Price Based', 'Percentage of order')}
          ${this._renderTypeOption('PerItem', 'icon-multiple', 'Per Item', 'Cost per item')}
          ${this._renderTypeOption('CarrierCalculated', 'icon-cloud', 'Carrier API', 'Real-time rates')}
        </div>
      </uui-box>

      ${this._renderRateConfiguration()}
      ${this._renderDeliveryEstimate()}
      ${this._renderRestrictions()}
    `;
  }

  _renderTypeOption(type, icon, name, desc) {
    return html`
      <div
        class="type-option ${this._method.calculationType === type ? 'selected' : ''}"
        @click=${() => this._handleTypeSelect(type)}
      >
        <uui-icon name="${icon}"></uui-icon>
        <span class="type-name">${name}</span>
        <span class="type-desc">${desc}</span>
      </div>
    `;
  }

  _renderRateConfiguration() {
    const type = this._method.calculationType;

    return html`
      <uui-box headline="Rate Configuration">
        <div class="rate-config">
          ${type === 'FlatRate' ? this._renderFlatRateConfig() : ''}
          ${type === 'FreeShipping' ? this._renderFreeShippingConfig() : ''}
          ${type === 'WeightBased' ? this._renderWeightBasedConfig() : ''}
          ${type === 'PriceBased' ? this._renderPriceBasedConfig() : ''}
          ${type === 'PerItem' ? this._renderPerItemConfig() : ''}
          ${type === 'CarrierCalculated' ? this._renderCarrierConfig() : ''}
        </div>
      </uui-box>
    `;
  }

  _renderFlatRateConfig() {
    return html`
      <div class="form-grid">
        <div class="form-group">
          <label>Flat Rate</label>
          <uui-input
            type="number"
            step="0.01"
            .value=${this._method.flatRate?.toString() || '0'}
            @input=${(e) => this._updateMethod('flatRate', parseFloat(e.target.value) || 0)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Handling Fee</label>
          <uui-input
            type="number"
            step="0.01"
            .value=${this._method.handlingFee?.toString() || '0'}
            @input=${(e) => this._updateMethod('handlingFee', parseFloat(e.target.value) || 0)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
      </div>
    `;
  }

  _renderFreeShippingConfig() {
    return html`
      <div class="form-grid">
        <div class="form-group">
          <label>Minimum Order Amount (Optional)</label>
          <uui-input
            type="number"
            step="0.01"
            placeholder="No minimum"
            .value=${this._method.freeShippingThreshold?.toString() || ''}
            @input=${(e) => this._updateMethod('freeShippingThreshold', e.target.value ? parseFloat(e.target.value) : null)}
          >
            <span slot="prepend">$</span>
          </uui-input>
          <span class="hint">Leave empty for always free</span>
        </div>
        <div class="form-group">
          <uui-toggle
            ?checked=${this._method.freeShippingRequiresCoupon}
            @change=${(e) => this._updateMethod('freeShippingRequiresCoupon', e.target.checked)}
          >
            Require Coupon Code
          </uui-toggle>
        </div>
      </div>
    `;
  }

  _renderWeightBasedConfig() {
    return html`
      <div class="form-grid">
        <div class="form-group">
          <label>Base Rate</label>
          <uui-input
            type="number"
            step="0.01"
            .value=${this._method.weightBaseRate?.toString() || '0'}
            @input=${(e) => this._updateMethod('weightBaseRate', parseFloat(e.target.value) || 0)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Rate Per Weight Unit</label>
          <uui-input
            type="number"
            step="0.01"
            .value=${this._method.weightPerUnitRate?.toString() || '0'}
            @input=${(e) => this._updateMethod('weightPerUnitRate', parseFloat(e.target.value) || 0)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Weight Unit</label>
          <uui-select
            .value=${this._method.weightUnit || 'kg'}
            @change=${(e) => this._updateMethod('weightUnit', e.target.value)}
          >
            <option value="kg">Kilograms (kg)</option>
            <option value="lb">Pounds (lb)</option>
            <option value="oz">Ounces (oz)</option>
            <option value="g">Grams (g)</option>
          </uui-select>
        </div>
      </div>
    `;
  }

  _renderPriceBasedConfig() {
    return html`
      <div class="form-grid">
        <div class="form-group">
          <label>Percentage of Order Total</label>
          <uui-input
            type="number"
            step="0.1"
            .value=${this._method.pricePercentage?.toString() || '0'}
            @input=${(e) => this._updateMethod('pricePercentage', parseFloat(e.target.value) || 0)}
          >
            <span slot="append">%</span>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Minimum Cost</label>
          <uui-input
            type="number"
            step="0.01"
            placeholder="No minimum"
            .value=${this._method.minimumCost?.toString() || ''}
            @input=${(e) => this._updateMethod('minimumCost', e.target.value ? parseFloat(e.target.value) : null)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Maximum Cost</label>
          <uui-input
            type="number"
            step="0.01"
            placeholder="No maximum"
            .value=${this._method.maximumCost?.toString() || ''}
            @input=${(e) => this._updateMethod('maximumCost', e.target.value ? parseFloat(e.target.value) : null)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
      </div>
    `;
  }

  _renderPerItemConfig() {
    return html`
      <div class="form-grid">
        <div class="form-group">
          <label>Rate Per Item</label>
          <uui-input
            type="number"
            step="0.01"
            .value=${this._method.perItemRate?.toString() || '0'}
            @input=${(e) => this._updateMethod('perItemRate', parseFloat(e.target.value) || 0)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
        <div class="form-group">
          <label>Handling Fee</label>
          <uui-input
            type="number"
            step="0.01"
            .value=${this._method.handlingFee?.toString() || '0'}
            @input=${(e) => this._updateMethod('handlingFee', parseFloat(e.target.value) || 0)}
          >
            <span slot="prepend">$</span>
          </uui-input>
        </div>
      </div>
    `;
  }

  _renderCarrierConfig() {
    return html`
      <div class="form-grid">
        <div class="form-group">
          <label>Carrier Provider</label>
          <uui-select
            .value=${this._method.carrierProviderId || ''}
            @change=${(e) => this._updateMethod('carrierProviderId', e.target.value)}
          >
            <option value="">Select carrier...</option>
            <option value="ups">UPS</option>
            <option value="fedex">FedEx</option>
            <option value="usps">USPS</option>
            <option value="dhl">DHL</option>
          </uui-select>
        </div>
        <div class="form-group">
          <label>Service Code</label>
          <uui-input
            placeholder="e.g., ground, express"
            .value=${this._method.carrierServiceCode || ''}
            @input=${(e) => this._updateMethod('carrierServiceCode', e.target.value)}
          ></uui-input>
        </div>
        <div class="form-group">
          <uui-toggle
            ?checked=${this._method.useCarrierRates}
            @change=${(e) => this._updateMethod('useCarrierRates', e.target.checked)}
          >
            Use Real-time Carrier Rates
          </uui-toggle>
        </div>
      </div>
    `;
  }

  _renderDeliveryEstimate() {
    return html`
      <uui-box headline="Delivery Estimate">
        <div class="form-grid">
          <div class="form-group">
            <label>Estimated Delivery Days</label>
            <div class="delivery-estimate">
              <uui-input
                type="number"
                min="0"
                placeholder="Min"
                .value=${this._method.estimatedDaysMin?.toString() || ''}
                @input=${(e) => this._updateMethod('estimatedDaysMin', e.target.value ? parseInt(e.target.value) : null)}
                style="width: 80px;"
              ></uui-input>
              <span>to</span>
              <uui-input
                type="number"
                min="0"
                placeholder="Max"
                .value=${this._method.estimatedDaysMax?.toString() || ''}
                @input=${(e) => this._updateMethod('estimatedDaysMax', e.target.value ? parseInt(e.target.value) : null)}
                style="width: 80px;"
              ></uui-input>
              <span>business days</span>
            </div>
          </div>
          <div class="form-group">
            <label>Custom Delivery Text</label>
            <uui-input
              placeholder="e.g., 3-5 business days"
              .value=${this._method.deliveryEstimateText || ''}
              @input=${(e) => this._updateMethod('deliveryEstimateText', e.target.value)}
            ></uui-input>
            <span class="hint">Overrides the calculated text if provided</span>
          </div>
        </div>
      </uui-box>
    `;
  }

  _renderRestrictions() {
    return html`
      <uui-box headline="Restrictions">
        <div class="form-grid">
          <div class="form-group">
            <label>Minimum Order Amount</label>
            <uui-input
              type="number"
              step="0.01"
              placeholder="No minimum"
              .value=${this._method.minOrderAmount?.toString() || ''}
              @input=${(e) => this._updateMethod('minOrderAmount', e.target.value ? parseFloat(e.target.value) : null)}
            >
              <span slot="prepend">$</span>
            </uui-input>
          </div>
          <div class="form-group">
            <label>Maximum Order Amount</label>
            <uui-input
              type="number"
              step="0.01"
              placeholder="No maximum"
              .value=${this._method.maxOrderAmount?.toString() || ''}
              @input=${(e) => this._updateMethod('maxOrderAmount', e.target.value ? parseFloat(e.target.value) : null)}
            >
              <span slot="prepend">$</span>
            </uui-input>
          </div>
          <div class="form-group">
            <label>Minimum Order Weight</label>
            <uui-input
              type="number"
              step="0.01"
              placeholder="No minimum"
              .value=${this._method.minWeight?.toString() || ''}
              @input=${(e) => this._updateMethod('minWeight', e.target.value ? parseFloat(e.target.value) : null)}
            ></uui-input>
          </div>
          <div class="form-group">
            <label>Maximum Order Weight</label>
            <uui-input
              type="number"
              step="0.01"
              placeholder="No maximum"
              .value=${this._method.maxWeight?.toString() || ''}
              @input=${(e) => this._updateMethod('maxWeight', e.target.value ? parseFloat(e.target.value) : null)}
            ></uui-input>
          </div>
          <div class="form-group">
            <uui-toggle
              ?checked=${this._method.isTaxable}
              @change=${(e) => this._updateMethod('isTaxable', e.target.checked)}
            >
              Shipping is Taxable
            </uui-toggle>
          </div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-shipping-method-editor', ShippingMethodEditor);

export default ShippingMethodEditor;
