import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Rates
 * Configure rates for each shipping method in this zone.
 */
export class ShippingZoneRates extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .rates-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .rates-table th,
    .rates-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .rates-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .rates-table tr:last-child td {
      border-bottom: none;
    }

    .rates-table tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .rate-input {
      width: 100px;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }

    .method-info {
      display: flex;
      flex-direction: column;
    }

    .method-name {
      font-weight: 500;
    }

    .method-type {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .status-toggle {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _zone: { type: Object, state: true },
    _rates: { type: Array, state: true },
    _methods: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._zone = null;
    this._rates = [];
    this._methods = [];
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    const workspace = this.closest('ecommerce-shipping-zone-workspace');
    if (workspace) {
      this._zone = workspace.getZone();
      if (this._zone?.id) {
        await this._loadRates();
      } else {
        this._loading = false;
      }
    }
  }

  async _loadRates() {
    try {
      this._loading = true;

      const [ratesResponse, methodsResponse] = await Promise.all([
        fetch(`/umbraco/management/api/v1/ecommerce/shipping/zone/${this._zone.id}/rates`, {
          headers: { 'Accept': 'application/json' }
        }),
        fetch('/umbraco/management/api/v1/ecommerce/shipping/method', {
          headers: { 'Accept': 'application/json' }
        })
      ]);

      if (ratesResponse.ok) {
        const ratesData = await ratesResponse.json();
        this._rates = ratesData.items || [];
      }

      if (methodsResponse.ok) {
        const methodsData = await methodsResponse.json();
        this._methods = methodsData.items || [];
      }
    } catch (error) {
      console.error('Error loading rates:', error);
    } finally {
      this._loading = false;
    }
  }

  async _updateRate(rateId, field, value) {
    try {
      await fetch(`/umbraco/management/api/v1/ecommerce/shipping/rate/${rateId}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ [field]: value })
      });
    } catch (error) {
      console.error('Error updating rate:', error);
    }
  }

  async _createRate(methodId) {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/shipping/rate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          shippingZoneId: this._zone.id,
          shippingMethodId: methodId,
          baseRate: 0,
          isActive: true
        })
      });

      if (response.ok) {
        await this._loadRates();
      }
    } catch (error) {
      console.error('Error creating rate:', error);
    }
  }

  _getMethodTypeLabel(type) {
    const labels = {
      'FlatRate': 'Flat Rate',
      'FreeShipping': 'Free Shipping',
      'WeightBased': 'Weight Based',
      'PriceBased': 'Price Based',
      'PerItem': 'Per Item',
      'CarrierCalculated': 'Carrier API'
    };
    return labels[type] || type;
  }

  render() {
    const isNewZone = !this._zone?.id;

    if (isNewZone) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-coins-dollar"></uui-icon>
          <h3>No rates configured</h3>
          <p>Save the shipping zone first to configure rates by method.</p>
        </div>
      `;
    }

    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    if (this._methods.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-truck"></uui-icon>
          <h3>No shipping methods</h3>
          <p>Create shipping methods first to configure rates for this zone.</p>
        </div>
      `;
    }

    return html`
      <uui-box headline="Rates by Method">
        <table class="rates-table">
          <thead>
            <tr>
              <th>Method</th>
              <th>Base Rate</th>
              <th>Handling Fee</th>
              <th>Free Shipping Threshold</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            ${this._methods.map(method => this._renderMethodRate(method))}
          </tbody>
        </table>
      </uui-box>
    `;
  }

  _renderMethodRate(method) {
    const rate = this._rates.find(r => r.shippingMethodId === method.id);

    return html`
      <tr>
        <td>
          <div class="method-info">
            <span class="method-name">${method.name}</span>
            <span class="method-type">${this._getMethodTypeLabel(method.calculationType)}</span>
          </div>
        </td>
        <td>
          ${rate ? html`
            <uui-input
              class="rate-input"
              type="number"
              step="0.01"
              .value=${rate.baseRate?.toString() || '0'}
              @change=${(e) => this._updateRate(rate.id, 'baseRate', parseFloat(e.target.value) || 0)}
            >
              <span slot="prepend">$</span>
            </uui-input>
          ` : '-'}
        </td>
        <td>
          ${rate ? html`
            <uui-input
              class="rate-input"
              type="number"
              step="0.01"
              .value=${rate.handlingFee?.toString() || '0'}
              @change=${(e) => this._updateRate(rate.id, 'handlingFee', parseFloat(e.target.value) || 0)}
            >
              <span slot="prepend">$</span>
            </uui-input>
          ` : '-'}
        </td>
        <td>
          ${rate ? html`
            <uui-input
              class="rate-input"
              type="number"
              step="0.01"
              placeholder="None"
              .value=${rate.freeShippingThreshold?.toString() || ''}
              @change=${(e) => this._updateRate(rate.id, 'freeShippingThreshold', e.target.value ? parseFloat(e.target.value) : null)}
            >
              <span slot="prepend">$</span>
            </uui-input>
          ` : '-'}
        </td>
        <td>
          ${rate ? html`
            <div class="status-toggle">
              <uui-toggle
                ?checked=${rate.isActive}
                @change=${(e) => this._updateRate(rate.id, 'isActive', e.target.checked)}
              ></uui-toggle>
            </div>
          ` : html`
            <uui-button look="secondary" compact @click=${() => this._createRate(method.id)}>
              Add Rate
            </uui-button>
          `}
        </td>
      </tr>
    `;
  }
}

customElements.define('ecommerce-shipping-zone-rates', ShippingZoneRates);

export default ShippingZoneRates;
