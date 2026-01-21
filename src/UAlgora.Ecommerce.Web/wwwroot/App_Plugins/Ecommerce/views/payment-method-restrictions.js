import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Restrictions
 * Editor for payment method restrictions.
 */
export class PaymentMethodRestrictions extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .editor-grid {
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

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    uui-input {
      width: 100%;
    }

    .tag-input {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-space-2);
    }

    .tag-input uui-input {
      flex: 1;
    }

    .tag-list {
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

    .tag uui-button {
      --uui-button-height: 20px;
      --uui-button-font-size: 12px;
    }

    .empty-list {
      color: var(--uui-color-text-alt);
      font-style: italic;
    }
  `;

  static properties = {
    _method: { type: Object, state: true },
    _newCountry: { type: String, state: true },
    _newExcludedCountry: { type: String, state: true },
    _newCurrency: { type: String, state: true },
    _newCustomerGroup: { type: String, state: true }
  };

  constructor() {
    super();
    this._method = {};
    this._newCountry = '';
    this._newExcludedCountry = '';
    this._newCurrency = '';
    this._newCustomerGroup = '';
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-payment-method-workspace');
      if (workspace) {
        this._method = workspace.getMethod();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateMethod(field, value) {
    this._method = { ...this._method, [field]: value };
    const workspace = this.closest('ecommerce-payment-method-workspace');
    if (workspace) {
      workspace.setMethod(this._method);
    }
  }

  _addToList(field, valueField) {
    const value = this[valueField]?.trim().toUpperCase();
    if (!value) return;

    const list = [...(this._method[field] || [])];
    if (!list.includes(value)) {
      list.push(value);
      this._updateMethod(field, list);
    }
    this[valueField] = '';
  }

  _removeFromList(field, value) {
    const list = (this._method[field] || []).filter(v => v !== value);
    this._updateMethod(field, list);
  }

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Order Amount Restrictions">
          <div class="form-group">
            <label>Minimum Order Amount ($)</label>
            <uui-input
              type="number"
              step="0.01"
              .value=${this._method.minOrderAmount || ''}
              @input=${(e) => this._updateMethod('minOrderAmount', parseFloat(e.target.value) || null)}
              placeholder="No minimum"
            ></uui-input>
            <p class="help-text">Orders below this amount cannot use this payment method</p>
          </div>

          <div class="form-group">
            <label>Maximum Order Amount ($)</label>
            <uui-input
              type="number"
              step="0.01"
              .value=${this._method.maxOrderAmount || ''}
              @input=${(e) => this._updateMethod('maxOrderAmount', parseFloat(e.target.value) || null)}
              placeholder="No maximum"
            ></uui-input>
            <p class="help-text">Orders above this amount cannot use this payment method</p>
          </div>
        </uui-box>

        <uui-box headline="Allowed Countries">
          <div class="form-group">
            <label>Allowed Countries (leave empty for all)</label>
            <div class="tag-input">
              <uui-input
                .value=${this._newCountry}
                @input=${(e) => this._newCountry = e.target.value}
                @keydown=${(e) => e.key === 'Enter' && this._addToList('allowedCountries', '_newCountry')}
                placeholder="Country code (e.g., US, GB)"
              ></uui-input>
              <uui-button
                look="secondary"
                @click=${() => this._addToList('allowedCountries', '_newCountry')}
              >Add</uui-button>
            </div>
            <div class="tag-list">
              ${(this._method.allowedCountries || []).length === 0
                ? html`<span class="empty-list">All countries allowed</span>`
                : (this._method.allowedCountries || []).map(c => html`
                    <span class="tag">
                      ${c}
                      <uui-button compact @click=${() => this._removeFromList('allowedCountries', c)}>
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </span>
                  `)
              }
            </div>
          </div>
        </uui-box>

        <uui-box headline="Excluded Countries">
          <div class="form-group">
            <label>Excluded Countries</label>
            <div class="tag-input">
              <uui-input
                .value=${this._newExcludedCountry}
                @input=${(e) => this._newExcludedCountry = e.target.value}
                @keydown=${(e) => e.key === 'Enter' && this._addToList('excludedCountries', '_newExcludedCountry')}
                placeholder="Country code (e.g., US, GB)"
              ></uui-input>
              <uui-button
                look="secondary"
                @click=${() => this._addToList('excludedCountries', '_newExcludedCountry')}
              >Add</uui-button>
            </div>
            <div class="tag-list">
              ${(this._method.excludedCountries || []).length === 0
                ? html`<span class="empty-list">No exclusions</span>`
                : (this._method.excludedCountries || []).map(c => html`
                    <span class="tag">
                      ${c}
                      <uui-button compact @click=${() => this._removeFromList('excludedCountries', c)}>
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </span>
                  `)
              }
            </div>
          </div>
        </uui-box>

        <uui-box headline="Allowed Currencies">
          <div class="form-group">
            <label>Allowed Currencies (leave empty for all)</label>
            <div class="tag-input">
              <uui-input
                .value=${this._newCurrency}
                @input=${(e) => this._newCurrency = e.target.value}
                @keydown=${(e) => e.key === 'Enter' && this._addToList('allowedCurrencies', '_newCurrency')}
                placeholder="Currency code (e.g., USD, EUR)"
              ></uui-input>
              <uui-button
                look="secondary"
                @click=${() => this._addToList('allowedCurrencies', '_newCurrency')}
              >Add</uui-button>
            </div>
            <div class="tag-list">
              ${(this._method.allowedCurrencies || []).length === 0
                ? html`<span class="empty-list">All currencies allowed</span>`
                : (this._method.allowedCurrencies || []).map(c => html`
                    <span class="tag">
                      ${c}
                      <uui-button compact @click=${() => this._removeFromList('allowedCurrencies', c)}>
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </span>
                  `)
              }
            </div>
          </div>
        </uui-box>

        <uui-box headline="Customer Groups">
          <div class="form-group">
            <label>Allowed Customer Groups (leave empty for all)</label>
            <div class="tag-input">
              <uui-input
                .value=${this._newCustomerGroup}
                @input=${(e) => this._newCustomerGroup = e.target.value}
                @keydown=${(e) => e.key === 'Enter' && this._addToList('allowedCustomerGroups', '_newCustomerGroup')}
                placeholder="Customer group name"
              ></uui-input>
              <uui-button
                look="secondary"
                @click=${() => this._addToList('allowedCustomerGroups', '_newCustomerGroup')}
              >Add</uui-button>
            </div>
            <div class="tag-list">
              ${(this._method.allowedCustomerGroups || []).length === 0
                ? html`<span class="empty-list">All customer groups allowed</span>`
                : (this._method.allowedCustomerGroups || []).map(g => html`
                    <span class="tag">
                      ${g}
                      <uui-button compact @click=${() => this._removeFromList('allowedCustomerGroups', g)}>
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </span>
                  `)
              }
            </div>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-payment-method-restrictions', PaymentMethodRestrictions);

export default PaymentMethodRestrictions;
