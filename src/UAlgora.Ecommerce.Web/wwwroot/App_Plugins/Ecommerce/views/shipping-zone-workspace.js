import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Workspace
 * Container for editing shipping zones.
 */
export class ShippingZoneWorkspace extends UmbElementMixin(LitElement) {
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

    .zone-name {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: 0;
    }

    .zone-code {
      font-family: monospace;
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .badges {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .badge {
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

    .region-summary {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _zone: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._zone = this._getDefaultZone();
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadZone();
  }

  _getDefaultZone() {
    return {
      name: '',
      description: '',
      code: '',
      isActive: true,
      isDefault: false,
      sortOrder: 0,
      countries: [],
      states: [],
      postalCodePatterns: [],
      cities: [],
      excludedCountries: [],
      excludedStates: [],
      excludedPostalCodes: []
    };
  }

  async _loadZone() {
    const pathParts = window.location.pathname.split('/');
    const editIndex = pathParts.indexOf('edit');

    if (editIndex !== -1 && pathParts[editIndex + 1]) {
      const zoneId = pathParts[editIndex + 1];
      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/zone/${zoneId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._zone = await response.json();
          this._isNew = false;
        }
      } catch (error) {
        console.error('Error loading shipping zone:', error);
      }
    }
  }

  getZone() {
    return this._zone;
  }

  setZone(zone) {
    this._zone = { ...this._zone, ...zone };
    this.requestUpdate();
  }

  isNewZone() {
    return this._isNew;
  }

  _getRegionSummary() {
    const parts = [];
    if (this._zone.countries?.length > 0) {
      parts.push(`${this._zone.countries.length} countries`);
    }
    if (this._zone.states?.length > 0) {
      parts.push(`${this._zone.states.length} states`);
    }
    if (this._zone.postalCodePatterns?.length > 0) {
      parts.push(`${this._zone.postalCodePatterns.length} postal patterns`);
    }
    return parts.length > 0 ? parts.join(', ') : 'All regions';
  }

  render() {
    return html`
      <div class="workspace-header">
        <uui-icon name="icon-globe" style="font-size: 32px;"></uui-icon>
        <div class="header-info">
          <h1 class="zone-name">
            ${this._zone.name || (this._isNew ? 'New Shipping Zone' : 'Loading...')}
          </h1>
          ${this._zone.code ? html`
            <span class="zone-code">${this._zone.code}</span>
          ` : ''}
        </div>
        <div class="region-summary">
          <uui-icon name="icon-map-location"></uui-icon>
          ${this._getRegionSummary()}
        </div>
        <div class="badges">
          ${this._zone.isDefault ? html`
            <span class="badge badge-default">Default Zone</span>
          ` : ''}
          <span class="badge ${this._zone.isActive ? 'status-active' : 'status-inactive'}">
            ${this._zone.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-shipping-zone-workspace', ShippingZoneWorkspace);

export default ShippingZoneWorkspace;
