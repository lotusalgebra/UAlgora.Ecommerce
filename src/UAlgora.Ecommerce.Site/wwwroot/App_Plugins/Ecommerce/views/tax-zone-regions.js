import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Zone Regions View
 * Configuration for geographic regions in a tax zone.
 */
export class TaxZoneRegions extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .regions-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group:last-child {
      margin-bottom: 0;
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: bold;
    }

    .form-group .hint {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
      margin-bottom: var(--uui-size-space-2);
    }

    .tag-input-container {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-space-2);
    }

    .tag-input-container uui-input {
      flex: 1;
    }

    .tags-list {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
    }

    .tag {
      display: inline-flex;
      align-items: center;
      gap: var(--uui-size-space-1);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .tag-remove {
      cursor: pointer;
      color: var(--uui-color-text-alt);
    }

    .tag-remove:hover {
      color: var(--uui-color-danger);
    }

    .exclusion-tag {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }

    .empty-state {
      color: var(--uui-color-text-alt);
      font-style: italic;
      padding: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _zone: { type: Object, state: true },
    _countryInput: { type: String, state: true },
    _stateInput: { type: String, state: true },
    _postalInput: { type: String, state: true },
    _cityInput: { type: String, state: true },
    _excludedCountryInput: { type: String, state: true },
    _excludedStateInput: { type: String, state: true },
    _excludedPostalInput: { type: String, state: true }
  };

  constructor() {
    super();
    this._zone = null;
    this._countryInput = '';
    this._stateInput = '';
    this._postalInput = '';
    this._cityInput = '';
    this._excludedCountryInput = '';
    this._excludedStateInput = '';
    this._excludedPostalInput = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-tax-zone-workspace');
    if (workspace) {
      this._zone = workspace.getZone();
    }
  }

  _updateField(field, value) {
    this._zone = { ...this._zone, [field]: value };
    const workspace = this.closest('ecommerce-tax-zone-workspace');
    if (workspace) {
      workspace._zone = this._zone;
    }
  }

  _addTag(field, inputField) {
    const value = this[inputField].trim().toUpperCase();
    if (value && !this._zone[field].includes(value)) {
      this._updateField(field, [...this._zone[field], value]);
    }
    this[inputField] = '';
  }

  _removeTag(field, value) {
    this._updateField(field, this._zone[field].filter(v => v !== value));
  }

  _renderTagInput(label, hint, field, inputField, placeholder, isExclusion = false) {
    return html`
      <div class="form-group">
        <label>${label}</label>
        <div class="hint">${hint}</div>
        <div class="tag-input-container">
          <uui-input
            .value=${this[inputField]}
            @input=${(e) => this[inputField] = e.target.value}
            @keypress=${(e) => e.key === 'Enter' && this._addTag(field, inputField)}
            placeholder=${placeholder}
          ></uui-input>
          <uui-button
            look="primary"
            @click=${() => this._addTag(field, inputField)}
          >Add</uui-button>
        </div>
        <div class="tags-list">
          ${this._zone[field]?.length > 0 ? this._zone[field].map(tag => html`
            <span class="tag ${isExclusion ? 'exclusion-tag' : ''}">
              ${tag}
              <uui-icon
                name="icon-delete"
                class="tag-remove"
                @click=${() => this._removeTag(field, tag)}
              ></uui-icon>
            </span>
          `) : html`<span class="empty-state">None added</span>`}
        </div>
      </div>
    `;
  }

  render() {
    if (!this._zone) {
      return html`<uui-loader></uui-loader>`;
    }

    return html`
      <div class="regions-grid">
        <uui-box headline="Included Regions">
          ${this._renderTagInput(
            'Countries',
            'ISO 3166-1 alpha-2 country codes (e.g., US, GB, DE)',
            'countries',
            '_countryInput',
            'Enter country code...'
          )}

          ${this._renderTagInput(
            'States/Provinces',
            'Format: CountryCode-StateCode (e.g., US-CA, US-NY, CA-ON)',
            'states',
            '_stateInput',
            'Enter state code...'
          )}

          ${this._renderTagInput(
            'Postal Code Patterns',
            'Use * for wildcards, - for ranges (e.g., 90*, 10001-10099)',
            'postalCodePatterns',
            '_postalInput',
            'Enter postal pattern...'
          )}

          ${this._renderTagInput(
            'Cities',
            'City names (case-insensitive)',
            'cities',
            '_cityInput',
            'Enter city name...'
          )}
        </uui-box>

        <uui-box headline="Excluded Regions">
          <p style="color: var(--uui-color-text-alt); margin-bottom: var(--uui-size-space-4);">
            Exclusions are checked before inclusions. Use these to carve out exceptions.
          </p>

          ${this._renderTagInput(
            'Excluded Countries',
            'Country codes to exclude from this zone',
            'excludedCountries',
            '_excludedCountryInput',
            'Enter country code...',
            true
          )}

          ${this._renderTagInput(
            'Excluded States',
            'State codes to exclude (same format as above)',
            'excludedStates',
            '_excludedStateInput',
            'Enter state code...',
            true
          )}

          ${this._renderTagInput(
            'Excluded Postal Codes',
            'Specific postal codes to exclude',
            'excludedPostalCodes',
            '_excludedPostalInput',
            'Enter postal code...',
            true
          )}
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-tax-zone-regions', TaxZoneRegions);

export default TaxZoneRegions;
