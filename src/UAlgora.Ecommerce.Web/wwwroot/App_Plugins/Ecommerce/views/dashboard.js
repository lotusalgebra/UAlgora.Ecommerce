import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * E-commerce Dashboard Element
 * Displays overview statistics and recent activity from the e-commerce system.
 */
export class EcommerceDashboard extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-2);
    }

    .stat-card {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-1);
    }

    .stat-card h3 {
      margin: 0 0 var(--uui-size-space-2) 0;
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
    }

    .stat-card .value {
      font-size: var(--uui-type-h2-size);
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
    }

    .loading {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--uui-size-layout-2);
    }
  `;

  static properties = {
    _loading: { type: Boolean, state: true },
    _overview: { type: Object, state: true },
    _error: { type: String, state: true }
  };

  constructor() {
    super();
    this._loading = true;
    this._overview = null;
    this._error = null;
  }

  async connectedCallback() {
    super.connectedCallback();
    await this._loadDashboardData();
  }

  async _loadDashboardData() {
    try {
      this._loading = true;

      const response = await fetch('/umbraco/management/api/v1/ecommerce/dashboard/overview', {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load dashboard data');
      }

      this._overview = await response.json();
      this._error = null;
    } catch (error) {
      this._error = error.message;
      console.error('Error loading dashboard:', error);
    } finally {
      this._loading = false;
    }
  }

  render() {
    if (this._loading) {
      return html`
        <div class="loading">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    if (this._error) {
      return html`
        <uui-box>
          <div slot="headline">Error</div>
          <p>${this._error}</p>
          <uui-button @click=${this._loadDashboardData}>Retry</uui-button>
        </uui-box>
      `;
    }

    return html`
      <uui-box>
        <div slot="headline">E-commerce Overview</div>

        <div class="dashboard-grid">
          <div class="stat-card">
            <h3>Total Orders</h3>
            <div class="value">${this._overview?.totalOrders ?? 0}</div>
          </div>

          <div class="stat-card">
            <h3>Pending Orders</h3>
            <div class="value">${this._overview?.pendingOrders ?? 0}</div>
          </div>

          <div class="stat-card">
            <h3>Today's Orders</h3>
            <div class="value">${this._overview?.todayOrders ?? 0}</div>
          </div>

          <div class="stat-card">
            <h3>Total Products</h3>
            <div class="value">${this._overview?.totalProducts ?? 0}</div>
          </div>

          <div class="stat-card">
            <h3>Total Customers</h3>
            <div class="value">${this._overview?.totalCustomers ?? 0}</div>
          </div>

          <div class="stat-card">
            <h3>Monthly Revenue</h3>
            <div class="value">
              ${this._formatCurrency(this._overview?.monthlyRevenue, this._overview?.currencyCode)}
            </div>
          </div>
        </div>
      </uui-box>

      <h2 class="section-title">Quick Actions</h2>
      <uui-box>
        <uui-button-group>
          <uui-button look="primary" label="Add Product">
            <uui-icon name="icon-add"></uui-icon>
            Add Product
          </uui-button>
          <uui-button look="secondary" label="View Orders">
            <uui-icon name="icon-receipt-dollar"></uui-icon>
            View Orders
          </uui-button>
          <uui-button look="secondary" label="Manage Customers">
            <uui-icon name="icon-users"></uui-icon>
            Customers
          </uui-button>
        </uui-button-group>
      </uui-box>
    `;
  }

  _formatCurrency(amount, currencyCode = 'USD') {
    if (amount == null) return '$0.00';

    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode
    }).format(amount);
  }
}

customElements.define('ecommerce-dashboard', EcommerceDashboard);

export default EcommerceDashboard;
