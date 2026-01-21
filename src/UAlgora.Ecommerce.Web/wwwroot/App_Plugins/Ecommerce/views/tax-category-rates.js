import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Rates View
 * Shows tax rates configured for this category across different zones.
 */
export class TaxCategoryRates extends UmbElementMixin(LitElement) {
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

    .jurisdiction-info {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _rates: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _categoryId: { type: String, state: true }
  };

  constructor() {
    super();
    this._rates = [];
    this._loading = true;
    this._categoryId = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadRates();
  }

  async _loadRates() {
    const workspace = this.closest('ecommerce-tax-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      this._loading = false;
      return;
    }

    this._categoryId = category.id;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${category.id}/rates`, {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        this._rates = data.items || [];
      }
    } catch (error) {
      console.error('Error loading tax rates:', error);
    } finally {
      this._loading = false;
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

    return html`
      <uui-box>
        <div class="rates-header">
          <h3>Tax Rates by Zone</h3>
        </div>

        ${this._rates.length === 0 ? html`
          <div class="empty-state">
            <uui-icon name="icon-percentage" style="font-size: 48px; margin-bottom: 16px;"></uui-icon>
            <h3>No tax rates configured</h3>
            <p>Create tax zones and add rates to configure taxation for this category.</p>
          </div>
        ` : html`
          <table class="rates-table">
            <thead>
              <tr>
                <th>Zone</th>
                <th>Rate</th>
                <th>Jurisdiction</th>
                <th>Effective Period</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              ${this._rates.map(rate => html`
                <tr>
                  <td>
                    <strong>${rate.taxZone?.name || 'Unknown Zone'}</strong>
                  </td>
                  <td>
                    <span class="rate-value">${this._formatRate(rate)}</span>
                    ${rate.isCompound ? html`<br><small>(compound)</small>` : ''}
                  </td>
                  <td>
                    ${rate.jurisdictionType || rate.jurisdictionName ? html`
                      <span class="jurisdiction-info">
                        ${rate.jurisdictionType ? `${rate.jurisdictionType}: ` : ''}${rate.jurisdictionName || '-'}
                      </span>
                    ` : '-'}
                  </td>
                  <td>
                    ${rate.effectiveFrom || rate.effectiveTo ? html`
                      ${rate.effectiveFrom ? new Date(rate.effectiveFrom).toLocaleDateString() : 'Start'}
                      -
                      ${rate.effectiveTo ? new Date(rate.effectiveTo).toLocaleDateString() : 'No end'}
                    ` : 'Always'}
                  </td>
                  <td>
                    <span class="status-badge ${rate.isActive ? 'status-active' : 'status-inactive'}">
                      ${rate.isActive ? 'Active' : 'Inactive'}
                    </span>
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

customElements.define('ecommerce-tax-category-rates', TaxCategoryRates);

export default TaxCategoryRates;
