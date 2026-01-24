import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Regions
 * Configure geographic regions for the zone.
 */
export class ShippingZoneRegions extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group label {
      display: block;
      font-weight: 500;
      margin-bottom: var(--uui-size-space-2);
    }

    .form-group .hint {
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
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .tag uui-button {
      padding: 0;
      min-width: auto;
    }

    .add-input {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .add-input uui-input {
      flex: 1;
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .section-title {
      font-size: var(--uui-type-h5-size);
      font-weight: bold;
      margin-bottom: var(--uui-size-space-4);
    }

    .country-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: var(--uui-size-space-2);
      max-height: 300px;
      overflow-y: auto;
      padding: var(--uui-size-space-2);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .country-option {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _zone: { type: Object, state: true },
    _newCountry: { type: String, state: true },
    _newState: { type: String, state: true },
    _newPostal: { type: String, state: true }
  };

  constructor() {
    super();
    this._zone = {};
    this._newCountry = '';
    this._newState = '';
    this._newPostal = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-shipping-zone-workspace');
    if (workspace) {
      this._zone = workspace.getZone();
    }
  }

  _updateZone(field, value) {
    const workspace = this.closest('ecommerce-shipping-zone-workspace');
    if (workspace) {
      workspace.setZone({ [field]: value });
      this._zone = workspace.getZone();
    }
  }

  _addCountry() {
    if (this._newCountry && !this._zone.countries?.includes(this._newCountry)) {
      this._updateZone('countries', [...(this._zone.countries || []), this._newCountry.toUpperCase()]);
      this._newCountry = '';
    }
  }

  _removeCountry(country) {
    this._updateZone('countries', this._zone.countries?.filter(c => c !== country) || []);
  }

  _addState() {
    if (this._newState && !this._zone.states?.includes(this._newState)) {
      this._updateZone('states', [...(this._zone.states || []), this._newState.toUpperCase()]);
      this._newState = '';
    }
  }

  _removeState(state) {
    this._updateZone('states', this._zone.states?.filter(s => s !== state) || []);
  }

  _addPostalPattern() {
    if (this._newPostal && !this._zone.postalCodePatterns?.includes(this._newPostal)) {
      this._updateZone('postalCodePatterns', [...(this._zone.postalCodePatterns || []), this._newPostal]);
      this._newPostal = '';
    }
  }

  _removePostalPattern(pattern) {
    this._updateZone('postalCodePatterns', this._zone.postalCodePatterns?.filter(p => p !== pattern) || []);
  }

  render() {
    return html`
      <uui-box headline="Countries">
        <div class="form-group">
          <label>Included Countries</label>
          <div class="tags-container">
            ${(this._zone.countries || []).map(country => html`
              <span class="tag">
                ${country}
                <uui-button compact look="secondary" @click=${() => this._removeCountry(country)}>
                  <uui-icon name="icon-delete"></uui-icon>
                </uui-button>
              </span>
            `)}
          </div>
          <div class="add-input">
            <uui-input
              placeholder="Country code (e.g., US, CA, GB)"
              .value=${this._newCountry}
              @input=${(e) => this._newCountry = e.target.value}
              @keypress=${(e) => e.key === 'Enter' && this._addCountry()}
            ></uui-input>
            <uui-button look="secondary" @click=${this._addCountry}>Add</uui-button>
          </div>
          <span class="hint">Use ISO 3166-1 alpha-2 country codes</span>
        </div>
      </uui-box>

      <uui-box headline="States/Provinces">
        <div class="form-group">
          <label>Included States</label>
          <div class="tags-container">
            ${(this._zone.states || []).map(state => html`
              <span class="tag">
                ${state}
                <uui-button compact look="secondary" @click=${() => this._removeState(state)}>
                  <uui-icon name="icon-delete"></uui-icon>
                </uui-button>
              </span>
            `)}
          </div>
          <div class="add-input">
            <uui-input
              placeholder="State code (e.g., US-CA, CA-ON)"
              .value=${this._newState}
              @input=${(e) => this._newState = e.target.value}
              @keypress=${(e) => e.key === 'Enter' && this._addState()}
            ></uui-input>
            <uui-button look="secondary" @click=${this._addState}>Add</uui-button>
          </div>
          <span class="hint">Format: COUNTRY-STATE (e.g., US-CA for California)</span>
        </div>
      </uui-box>

      <uui-box headline="Postal Codes">
        <div class="form-group">
          <label>Postal Code Patterns</label>
          <div class="tags-container">
            ${(this._zone.postalCodePatterns || []).map(pattern => html`
              <span class="tag">
                ${pattern}
                <uui-button compact look="secondary" @click=${() => this._removePostalPattern(pattern)}>
                  <uui-icon name="icon-delete"></uui-icon>
                </uui-button>
              </span>
            `)}
          </div>
          <div class="add-input">
            <uui-input
              placeholder="Postal pattern (e.g., 90*, 10001-10099)"
              .value=${this._newPostal}
              @input=${(e) => this._newPostal = e.target.value}
              @keypress=${(e) => e.key === 'Enter' && this._addPostalPattern()}
            ></uui-input>
            <uui-button look="secondary" @click=${this._addPostalPattern}>Add</uui-button>
          </div>
          <span class="hint">Use * for wildcards, or ranges like 10001-10099</span>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-shipping-zone-regions', ShippingZoneRegions);

export default ShippingZoneRegions;
