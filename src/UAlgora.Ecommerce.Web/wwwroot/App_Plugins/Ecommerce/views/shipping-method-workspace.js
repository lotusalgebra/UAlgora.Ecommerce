import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Workspace
 * Container for editing shipping methods.
 */
export class ShippingMethodWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
    }

    .workspace-header {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
      padding: var(--uui-size-layout-1);
      border-bottom: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
    }

    .header-info {
      flex: 1;
    }

    .method-name {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: 0;
    }

    .method-code {
      font-family: monospace;
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .status-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .status-inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .calculation-type {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .calculation-type uui-icon {
      color: var(--uui-color-interactive);
    }
  `;

  static properties = {
    _method: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._method = this._getDefaultMethod();
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadMethod();
  }

  _getDefaultMethod() {
    return {
      name: '',
      description: '',
      code: '',
      calculationType: 'FlatRate',
      isActive: true,
      sortOrder: 0,
      flatRate: 0,
      weightBaseRate: 0,
      weightPerUnitRate: 0,
      weightUnit: 'kg',
      pricePercentage: 0,
      minimumCost: null,
      maximumCost: null,
      perItemRate: 0,
      handlingFee: 0,
      freeShippingThreshold: null,
      freeShippingRequiresCoupon: false,
      minWeight: null,
      maxWeight: null,
      minOrderAmount: null,
      maxOrderAmount: null,
      estimatedDaysMin: null,
      estimatedDaysMax: null,
      deliveryEstimateText: '',
      carrierProviderId: null,
      carrierServiceCode: null,
      useCarrierRates: false,
      iconName: 'icon-truck',
      isTaxable: true
    };
  }

  async _loadMethod() {
    const pathParts = window.location.pathname.split('/');
    const editIndex = pathParts.indexOf('edit');

    if (editIndex !== -1 && pathParts[editIndex + 1]) {
      const methodId = pathParts[editIndex + 1];
      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/method/${methodId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._method = await response.json();
          this._isNew = false;
        }
      } catch (error) {
        console.error('Error loading shipping method:', error);
      }
    }
  }

  getMethod() {
    return this._method;
  }

  setMethod(method) {
    this._method = { ...this._method, ...method };
    this.requestUpdate();
  }

  isNewMethod() {
    return this._isNew;
  }

  _getCalculationTypeLabel(type) {
    const labels = {
      'FlatRate': 'Flat Rate',
      'FreeShipping': 'Free Shipping',
      'WeightBased': 'Weight Based',
      'PriceBased': 'Price Based',
      'PerItem': 'Per Item',
      'CarrierCalculated': 'Carrier Calculated'
    };
    return labels[type] || type;
  }

  _getCalculationTypeIcon(type) {
    const icons = {
      'FlatRate': 'icon-coin',
      'FreeShipping': 'icon-gift',
      'WeightBased': 'icon-scale',
      'PriceBased': 'icon-coins-dollar',
      'PerItem': 'icon-multiple',
      'CarrierCalculated': 'icon-cloud'
    };
    return icons[type] || 'icon-truck';
  }

  render() {
    return html`
      <div class="workspace-header">
        <uui-icon name="icon-truck" style="font-size: 32px;"></uui-icon>
        <div class="header-info">
          <h1 class="method-name">
            ${this._method.name || (this._isNew ? 'New Shipping Method' : 'Loading...')}
          </h1>
          ${this._method.code ? html`
            <span class="method-code">${this._method.code}</span>
          ` : ''}
        </div>
        <div class="calculation-type">
          <uui-icon name="${this._getCalculationTypeIcon(this._method.calculationType)}"></uui-icon>
          ${this._getCalculationTypeLabel(this._method.calculationType)}
        </div>
        <span class="status-badge ${this._method.isActive ? 'status-active' : 'status-inactive'}">
          ${this._method.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-shipping-method-workspace', ShippingMethodWorkspace);

export default ShippingMethodWorkspace;
