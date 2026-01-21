import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Zone Workspace
 * Main container for editing tax zones.
 */
export class TaxZoneWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      height: 100%;
    }

    .workspace-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-layout-1);
      border-bottom: 1px solid var(--uui-color-border);
      background: var(--uui-color-surface);
    }

    .header-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-4);
    }

    .header-info uui-icon {
      font-size: 24px;
      color: var(--uui-color-interactive);
    }

    .header-title h1 {
      margin: 0;
      font-size: var(--uui-type-h4-size);
    }

    .header-title .subtitle {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
    }

    .header-actions {
      display: flex;
      gap: var(--uui-size-space-3);
    }

    .status-badge {
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
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .status-default {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .loading {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 200px;
    }
  `;

  static properties = {
    _zone: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._zone = this._getDefaultZone();
    this._loading = true;
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadZone();
  }

  _getDefaultZone() {
    return {
      id: null,
      name: '',
      code: '',
      description: '',
      isActive: true,
      isDefault: false,
      priority: 0,
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
    const path = window.location.pathname;
    const match = path.match(/\/edit\/([a-f0-9-]+)/i);

    if (match) {
      this._isNew = false;
      const zoneId = match[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/zone/${zoneId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._zone = await response.json();
        }
      } catch (error) {
        console.error('Error loading tax zone:', error);
      }
    }

    this._loading = false;
  }

  getZone() {
    return { ...this._zone };
  }

  setZone(zone) {
    this._zone = { ...zone };
    this._isNew = false;
  }

  isNewZone() {
    return this._isNew;
  }

  render() {
    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-globe"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Tax Zone' : this._zone.name}</h1>
            <span class="subtitle">${this._isNew ? 'Create a new tax zone' : `Code: ${this._zone.code}`}</span>
          </div>
          ${!this._isNew ? html`
            <span class="status-badge ${this._zone.isActive ? 'status-active' : 'status-inactive'}">
              ${this._zone.isActive ? 'Active' : 'Inactive'}
            </span>
            ${this._zone.isDefault ? html`<span class="status-badge status-default">Default</span>` : ''}
          ` : ''}
        </div>
        <div class="header-actions">
          <ecommerce-tax-zone-save-action></ecommerce-tax-zone-save-action>
        </div>
      </div>
      <umb-body-layout>
        <slot></slot>
      </umb-body-layout>
    `;
  }
}

customElements.define('ecommerce-tax-zone-workspace', TaxZoneWorkspace);

export default TaxZoneWorkspace;
