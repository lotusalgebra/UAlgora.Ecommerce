import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Rates View
 * Displays rates for this method across different zones.
 */
export class ShippingMethodRates extends UmbElementMixin(LitElement) {
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
  `;

  static properties = {
    _method: { type: Object, state: true },
    _rates: { type: Array, state: true },
    _zones: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._method = null;
    this._rates = [];
    this._zones = [];
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    const workspace = this.closest('ecommerce-shipping-method-workspace');
    if (workspace) {
      this._method = workspace.getMethod();
      if (this._method?.id) {
        await this._loadRates();
      } else {
        this._loading = false;
      }
    }
  }

  async _loadRates() {
    try {
      this._loading = true;

      const [ratesResponse, zonesResponse] = await Promise.all([
        fetch(`/umbraco/management/api/v1/ecommerce/shipping/method/${this._method.id}/rates`, {
          headers: { 'Accept': 'application/json' }
        }),
        fetch('/umbraco/management/api/v1/ecommerce/shipping/zone', {
          headers: { 'Accept': 'application/json' }
        })
      ]);

      if (ratesResponse.ok) {
        const ratesData = await ratesResponse.json();
        this._rates = ratesData.items || [];
      }

      if (zonesResponse.ok) {
        const zonesData = await zonesResponse.json();
        this._zones = zonesData.items || [];
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

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount || 0);
  }

  render() {
    const isNewMethod = !this._method?.id;

    if (isNewMethod) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-coins-dollar"></uui-icon>
          <h3>No rates configured</h3>
          <p>Save the shipping method first to configure rates by zone.</p>
        </div>
      `;
    }

    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    if (this._zones.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-globe"></uui-icon>
          <h3>No shipping zones</h3>
          <p>Create shipping zones first to configure rates for this method.</p>
        </div>
      `;
    }

    return html`
      <uui-box headline="Rates by Zone">
        <table class="rates-table">
          <thead>
            <tr>
              <th>Zone</th>
              <th>Base Rate</th>
              <th>Handling Fee</th>
              <th>Free Shipping Threshold</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            ${this._zones.map(zone => this._renderZoneRate(zone))}
          </tbody>
        </table>
      </uui-box>
    `;
  }

  _renderZoneRate(zone) {
    const rate = this._rates.find(r => r.shippingZoneId === zone.id);

    return html`
      <tr>
        <td>
          <strong>${zone.name}</strong>
          ${zone.isDefault ? html`<small>(Default)</small>` : ''}
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
              placeholder="No threshold"
              .value=${rate.freeShippingThreshold?.toString() || ''}
              @change=${(e) => this._updateRate(rate.id, 'freeShippingThreshold', e.target.value ? parseFloat(e.target.value) : null)}
            >
              <span slot="prepend">$</span>
            </uui-input>
          ` : '-'}
        </td>
        <td>
          ${rate ? html`
            <span class="status-badge ${rate.isActive ? 'status-active' : 'status-inactive'}">
              ${rate.isActive ? 'Active' : 'Inactive'}
            </span>
          ` : html`
            <uui-button look="secondary" compact @click=${() => this._createRate(zone.id)}>
              Add Rate
            </uui-button>
          `}
        </td>
      </tr>
    `;
  }

  async _createRate(zoneId) {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/shipping/rate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          shippingZoneId: zoneId,
          shippingMethodId: this._method.id,
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
}

customElements.define('ecommerce-shipping-method-rates', ShippingMethodRates);

export default ShippingMethodRates;
