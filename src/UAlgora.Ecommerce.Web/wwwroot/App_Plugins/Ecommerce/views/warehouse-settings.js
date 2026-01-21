import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Settings
 * Advanced warehouse settings and configuration.
 */
export class WarehouseSettings extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .settings-grid {
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

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    uui-input, uui-textarea, uui-select {
      width: 100%;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .countries-list {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-space-2);
    }

    .country-tag {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-1);
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .country-tag uui-button {
      --uui-button-height: auto;
      --uui-button-padding-left-factor: 0;
      --uui-button-padding-right-factor: 0;
    }

    .add-country {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-space-3);
    }

    .add-country uui-input {
      width: 100px;
    }

    .hours-grid {
      display: grid;
      grid-template-columns: auto 1fr 1fr;
      gap: var(--uui-size-space-3);
      align-items: center;
    }

    .hours-grid .day-label {
      font-weight: 500;
    }
  `;

  static properties = {
    _warehouse: { type: Object, state: true },
    _newCountry: { type: String, state: true }
  };

  constructor() {
    super();
    this._warehouse = {};
    this._newCountry = '';
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-warehouse-workspace');
      if (workspace) {
        this._warehouse = workspace.getWarehouse();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateWarehouse(field, value) {
    this._warehouse = { ...this._warehouse, [field]: value };
    const workspace = this.closest('ecommerce-warehouse-workspace');
    if (workspace) {
      workspace.setWarehouse(this._warehouse);
    }
  }

  _addCountry() {
    if (!this._newCountry || this._newCountry.length !== 2) return;

    const code = this._newCountry.toUpperCase();
    const countries = this._warehouse.shippingCountries || [];

    if (!countries.includes(code)) {
      this._updateWarehouse('shippingCountries', [...countries, code]);
    }

    this._newCountry = '';
  }

  _removeCountry(code) {
    const countries = this._warehouse.shippingCountries || [];
    this._updateWarehouse('shippingCountries', countries.filter(c => c !== code));
  }

  _updateOperatingHours(day, field, value) {
    const hours = { ...(this._warehouse.operatingHours || {}) };
    const dayHours = hours[day] || '';

    if (field === 'open') {
      const close = dayHours.split('-')[1] || '';
      hours[day] = `${value}-${close}`;
    } else {
      const open = dayHours.split('-')[0] || '';
      hours[day] = `${open}-${value}`;
    }

    this._updateWarehouse('operatingHours', hours);
  }

  _getDayHours(day) {
    const hours = this._warehouse.operatingHours || {};
    const dayHours = hours[day] || '';
    const parts = dayHours.split('-');
    return {
      open: parts[0] || '',
      close: parts[1] || ''
    };
  }

  render() {
    const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
    const countries = this._warehouse.shippingCountries || [];

    return html`
      <div class="settings-grid">
        <uui-box headline="Shipping Countries">
          <p class="help-text">Countries this warehouse can ship to. Leave empty to allow all countries.</p>

          <div class="countries-list">
            ${countries.map(code => html`
              <span class="country-tag">
                ${code}
                <uui-button compact @click=${() => this._removeCountry(code)}>
                  <uui-icon name="icon-remove"></uui-icon>
                </uui-button>
              </span>
            `)}
            ${countries.length === 0 ? html`<span class="help-text">All countries</span>` : ''}
          </div>

          <div class="add-country">
            <uui-input
              .value=${this._newCountry}
              @input=${(e) => this._newCountry = e.target.value}
              placeholder="XX"
              maxlength="2"
              @keypress=${(e) => e.key === 'Enter' && this._addCountry()}
            ></uui-input>
            <uui-button look="secondary" @click=${this._addCountry}>
              Add Country
            </uui-button>
          </div>
        </uui-box>

        <uui-box headline="Operating Hours">
          <p class="help-text">Set operating hours for this warehouse (24-hour format)</p>

          <div class="hours-grid">
            ${days.map(day => {
              const hours = this._getDayHours(day);
              return html`
                <span class="day-label">${day}</span>
                <uui-input
                  type="time"
                  .value=${hours.open}
                  @input=${(e) => this._updateOperatingHours(day, 'open', e.target.value)}
                  placeholder="Open"
                ></uui-input>
                <uui-input
                  type="time"
                  .value=${hours.close}
                  @input=${(e) => this._updateOperatingHours(day, 'close', e.target.value)}
                  placeholder="Close"
                ></uui-input>
              `;
            })}
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-warehouse-settings', WarehouseSettings);

export default WarehouseSettings;
