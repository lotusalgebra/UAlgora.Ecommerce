import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Zone Rates View
 * Configuration for tax rates in a zone by category.
 */
export class TaxZoneRates extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .rates-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-5);
    }

    .rates-table {
      width: 100%;
      border-collapse: collapse;
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

    .rates-table tr:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .rate-input {
      width: 100px;
    }

    .rate-value {
      font-family: monospace;
      font-size: 1.1em;
    }

    .status-badge {
      display: inline-block;
      padding: var(--uui-size-space-1) var(--uui-size-space-3);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
    }

    .status-active {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .status-inactive {
      background: var(--uui-color-surface-alt);
      color: var(--uui-color-text-alt);
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }

    .rate-actions {
      display: flex;
      gap: var(--uui-size-space-2);
    }

    .add-rate-form {
      display: grid;
      grid-template-columns: 1fr 1fr 120px 100px auto;
      gap: var(--uui-size-space-3);
      align-items: end;
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-top: var(--uui-size-space-4);
    }

    .form-field label {
      display: block;
      margin-bottom: var(--uui-size-space-1);
      font-size: var(--uui-type-small-size);
    }
  `;

  static properties = {
    _rates: { type: Array, state: true },
    _categories: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _zoneId: { type: String, state: true },
    _showAddForm: { type: Boolean, state: true },
    _newRate: { type: Object, state: true }
  };

  constructor() {
    super();
    this._rates = [];
    this._categories = [];
    this._loading = true;
    this._zoneId = null;
    this._showAddForm = false;
    this._newRate = this._getDefaultRate();
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  _getDefaultRate() {
    return {
      taxCategoryId: '',
      rate: 0,
      rateType: 'Percentage',
      isActive: true
    };
  }

  async _loadData() {
    const workspace = this.closest('ecommerce-tax-zone-workspace');
    if (!workspace) return;

    const zone = workspace.getZone();
    if (!zone?.id) {
      this._loading = false;
      return;
    }

    this._zoneId = zone.id;

    try {
      const [ratesResponse, categoriesResponse] = await Promise.all([
        fetch(`/umbraco/management/api/v1/ecommerce/tax/zone/${zone.id}/rates`, {
          headers: { 'Accept': 'application/json' }
        }),
        fetch('/umbraco/management/api/v1/ecommerce/tax/category?includeInactive=false', {
          headers: { 'Accept': 'application/json' }
        })
      ]);

      if (ratesResponse.ok) {
        const data = await ratesResponse.json();
        this._rates = data.items || [];
      }

      if (categoriesResponse.ok) {
        const data = await categoriesResponse.json();
        this._categories = data.items || [];
      }
    } catch (error) {
      console.error('Error loading tax rates:', error);
    } finally {
      this._loading = false;
    }
  }

  async _addRate() {
    if (!this._newRate.taxCategoryId || !this._zoneId) return;

    try {
      const category = this._categories.find(c => c.id === this._newRate.taxCategoryId);
      const zone = this.closest('ecommerce-tax-zone-workspace')?.getZone();

      const response = await fetch('/umbraco/management/api/v1/ecommerce/tax/rate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          name: `${zone?.name || 'Zone'} - ${category?.name || 'Category'}`,
          taxZoneId: this._zoneId,
          taxCategoryId: this._newRate.taxCategoryId,
          rate: this._newRate.rate,
          rateType: this._newRate.rateType,
          isActive: this._newRate.isActive
        })
      });

      if (response.ok) {
        this._showAddForm = false;
        this._newRate = this._getDefaultRate();
        await this._loadData();
      }
    } catch (error) {
      console.error('Error adding rate:', error);
    }
  }

  async _deleteRate(rateId) {
    if (!confirm('Are you sure you want to delete this rate?')) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/rate/${rateId}`, {
        method: 'DELETE'
      });

      if (response.ok) {
        await this._loadData();
      }
    } catch (error) {
      console.error('Error deleting rate:', error);
    }
  }

  _formatRate(rate) {
    if (rate.rateType === 'Percentage') {
      return `${rate.rate}%`;
    } else if (rate.rateType === 'FlatRate') {
      return `$${rate.flatAmount?.toFixed(2) || '0.00'}`;
    }
    return `$${rate.flatAmount?.toFixed(2) || '0.00'}/unit`;
  }

  render() {
    if (this._loading) {
      return html`<div class="loading"><uui-loader></uui-loader></div>`;
    }

    const workspace = this.closest('ecommerce-tax-zone-workspace');
    const isNew = workspace?.isNewZone();

    if (isNew) {
      return html`
        <uui-box>
          <div class="empty-state">
            <uui-icon name="icon-percentage" style="font-size: 48px; margin-bottom: 16px;"></uui-icon>
            <h3>Save the zone first</h3>
            <p>Save this tax zone before adding rates.</p>
          </div>
        </uui-box>
      `;
    }

    return html`
      <uui-box>
        <div class="rates-header">
          <h3>Tax Rates by Category</h3>
          <uui-button look="primary" @click=${() => this._showAddForm = !this._showAddForm}>
            <uui-icon name="icon-add"></uui-icon>
            Add Rate
          </uui-button>
        </div>

        ${this._showAddForm ? html`
          <div class="add-rate-form">
            <div class="form-field">
              <label>Category</label>
              <uui-select
                .value=${this._newRate.taxCategoryId}
                @change=${(e) => this._newRate = {...this._newRate, taxCategoryId: e.target.value}}
              >
                <option value="">Select category...</option>
                ${this._categories.map(cat => html`
                  <option value=${cat.id}>${cat.name}</option>
                `)}
              </uui-select>
            </div>
            <div class="form-field">
              <label>Type</label>
              <uui-select
                .value=${this._newRate.rateType}
                @change=${(e) => this._newRate = {...this._newRate, rateType: e.target.value}}
              >
                <option value="Percentage">Percentage</option>
                <option value="FlatRate">Flat Rate</option>
                <option value="PerUnit">Per Unit</option>
              </uui-select>
            </div>
            <div class="form-field">
              <label>Rate</label>
              <uui-input
                type="number"
                step="0.01"
                .value=${this._newRate.rate.toString()}
                @input=${(e) => this._newRate = {...this._newRate, rate: parseFloat(e.target.value) || 0}}
              ></uui-input>
            </div>
            <div class="form-field">
              <label>Active</label>
              <uui-toggle
                .checked=${this._newRate.isActive}
                @change=${(e) => this._newRate = {...this._newRate, isActive: e.target.checked}}
              ></uui-toggle>
            </div>
            <uui-button look="primary" @click=${this._addRate}>Add</uui-button>
          </div>
        ` : ''}

        ${this._rates.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-percentage" style="font-size: 48px; margin-bottom: 16px;"></uui-icon>
            <h3>No tax rates configured</h3>
            <p>Add rates for different tax categories in this zone.</p>
          </div>
        ` : html`
          <table class="rates-table">
            <thead>
              <tr>
                <th>Category</th>
                <th>Rate</th>
                <th>Type</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              ${this._rates.map(rate => html`
                <tr>
                  <td>
                    <strong>${rate.taxCategory?.name || 'Unknown'}</strong>
                    ${rate.taxCategory?.isTaxExempt ? html`<br><small>(exempt)</small>` : ''}
                  </td>
                  <td>
                    <span class="rate-value">${this._formatRate(rate)}</span>
                    ${rate.isCompound ? html`<br><small>(compound)</small>` : ''}
                  </td>
                  <td>${rate.rateType}</td>
                  <td>
                    <span class="status-badge ${rate.isActive ? 'status-active' : 'status-inactive'}">
                      ${rate.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>
                    <div class="rate-actions">
                      <uui-button look="secondary" compact @click=${() => this._deleteRate(rate.id)}>
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </div>
                  </td>
                </tr>
              `)}
            </tbody>
          </table>
        `}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-tax-zone-rates', TaxZoneRates);

export default TaxZoneRates;
