import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Collection View
 * Displays overview of shipping methods and zones.
 */
export class ShippingCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .collection-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-4);
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
    }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .collection-table th,
    .collection-table td {
      padding: var(--uui-size-space-4);
      text-align: left;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .collection-table th {
      background: var(--uui-color-surface-alt);
      font-weight: bold;
    }

    .collection-table tr:last-child td {
      border-bottom: none;
    }

    .collection-table tr:hover {
      background: var(--uui-color-surface-emphasis);
      cursor: pointer;
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

    .badge-default {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .type-badge {
      font-family: monospace;
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
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

    .region-count {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _methods: { type: Array, state: true },
    _zones: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._methods = [];
    this._zones = [];
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    try {
      this._loading = true;

      const [methodsResponse, zonesResponse] = await Promise.all([
        fetch('/umbraco/management/api/v1/ecommerce/shipping/method?includeInactive=true', {
          headers: { 'Accept': 'application/json' }
        }),
        fetch('/umbraco/management/api/v1/ecommerce/shipping/zone?includeInactive=true', {
          headers: { 'Accept': 'application/json' }
        })
      ]);

      if (methodsResponse.ok) {
        const methodsData = await methodsResponse.json();
        this._methods = methodsData.items || [];
      }

      if (zonesResponse.ok) {
        const zonesData = await zonesResponse.json();
        this._zones = zonesData.items || [];
      }
    } catch (error) {
      console.error('Error loading shipping data:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleMethodClick(method) {
    this._navigateTo(`/section/ecommerce/workspace/shipping-method/edit/${method.id}`);
  }

  _handleZoneClick(zone) {
    this._navigateTo(`/section/ecommerce/workspace/shipping-zone/edit/${zone.id}`);
  }

  _handleCreateMethod() {
    this._navigateTo('/section/ecommerce/workspace/shipping-method/create');
  }

  _handleCreateZone() {
    this._navigateTo('/section/ecommerce/workspace/shipping-zone/create');
  }

  _navigateTo(path) {
    window.history.pushState(null, '', `/umbraco${path}`);
    const event = new CustomEvent('umb-navigate', {
      bubbles: true,
      composed: true,
      detail: { path }
    });
    this.dispatchEvent(event);
  }

  _getTypeLabel(type) {
    const labels = {
      'FlatRate': 'Flat Rate',
      'FreeShipping': 'Free',
      'WeightBased': 'Weight',
      'PriceBased': 'Price %',
      'PerItem': 'Per Item',
      'CarrierCalculated': 'Carrier'
    };
    return labels[type] || type;
  }

  _getRegionCount(zone) {
    const count = (zone.countries?.length || 0) +
                  (zone.states?.length || 0) +
                  (zone.postalCodePatterns?.length || 0);
    return count > 0 ? `${count} regions` : 'All regions';
  }

  render() {
    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    return html`
      <div class="collection-grid">
        <uui-box>
          <div class="section-header">
            <h2 class="section-title">Shipping Methods</h2>
            <uui-button look="primary" @click=${this._handleCreateMethod}>
              <uui-icon name="icon-add"></uui-icon>
              Add Method
            </uui-button>
          </div>

          ${this._methods.length === 0
            ? this._renderEmptyState('truck', 'No shipping methods', 'Create your first shipping method to get started')
            : this._renderMethodsTable()
          }
        </uui-box>

        <uui-box>
          <div class="section-header">
            <h2 class="section-title">Shipping Zones</h2>
            <uui-button look="primary" @click=${this._handleCreateZone}>
              <uui-icon name="icon-add"></uui-icon>
              Add Zone
            </uui-button>
          </div>

          ${this._zones.length === 0
            ? this._renderEmptyState('globe', 'No shipping zones', 'Create shipping zones to define geographic regions')
            : this._renderZonesTable()
          }
        </uui-box>
      </div>
    `;
  }

  _renderEmptyState(icon, title, message) {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-${icon}"></uui-icon>
        <h3>${title}</h3>
        <p>${message}</p>
      </div>
    `;
  }

  _renderMethodsTable() {
    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Type</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          ${this._methods.map(method => html`
            <tr @click=${() => this._handleMethodClick(method)}>
              <td><strong>${method.name}</strong></td>
              <td><span class="type-badge">${this._getTypeLabel(method.calculationType)}</span></td>
              <td>
                <span class="status-badge ${method.isActive ? 'status-active' : 'status-inactive'}">
                  ${method.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }

  _renderZonesTable() {
    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Regions</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          ${this._zones.map(zone => html`
            <tr @click=${() => this._handleZoneClick(zone)}>
              <td>
                <strong>${zone.name}</strong>
                ${zone.isDefault ? html`<span class="status-badge badge-default" style="margin-left: 8px;">Default</span>` : ''}
              </td>
              <td><span class="region-count">${this._getRegionCount(zone)}</span></td>
              <td>
                <span class="status-badge ${zone.isActive ? 'status-active' : 'status-inactive'}">
                  ${zone.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }
}

customElements.define('ecommerce-shipping-collection', ShippingCollection);

export default ShippingCollection;
