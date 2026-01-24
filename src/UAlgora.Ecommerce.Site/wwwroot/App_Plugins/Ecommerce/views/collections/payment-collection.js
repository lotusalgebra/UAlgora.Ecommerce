import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Collection
 * Combined collection view for payment methods and gateways.
 */
export class PaymentCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .tabs {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-layout-1);
      border-bottom: 1px solid var(--uui-color-border);
      padding-bottom: var(--uui-size-space-3);
    }

    .tab {
      padding: var(--uui-size-space-3) var(--uui-size-space-5);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
      font-weight: 500;
    }

    .tab:hover {
      background: var(--uui-color-surface-alt);
    }

    .tab.active {
      background: var(--uui-color-selected);
      color: var(--uui-color-selected-contrast);
    }

    .toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
    }

    .search-box {
      width: 300px;
    }

    uui-table {
      width: 100%;
    }

    uui-table-head-cell {
      font-weight: 600;
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

    .badge-sandbox {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .badge-live {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .actions {
      display: flex;
      gap: var(--uui-size-space-2);
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
  `;

  static properties = {
    _activeTab: { type: String, state: true },
    _methods: { type: Array, state: true },
    _gateways: { type: Array, state: true },
    _searchTerm: { type: String, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._activeTab = 'methods';
    this._methods = [];
    this._gateways = [];
    this._searchTerm = '';
    this._loading = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    this._loading = true;
    await Promise.all([
      this._loadMethods(),
      this._loadGateways()
    ]);
    this._loading = false;
  }

  async _loadMethods() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/payment/methods', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._methods = await response.json();
      }
    } catch (error) {
      console.error('Error loading payment methods:', error);
    }
  }

  async _loadGateways() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/payment/gateways', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._gateways = await response.json();
      }
    } catch (error) {
      console.error('Error loading payment gateways:', error);
    }
  }

  get _filteredMethods() {
    if (!this._searchTerm) return this._methods;
    const term = this._searchTerm.toLowerCase();
    return this._methods.filter(m =>
      m.name?.toLowerCase().includes(term) ||
      m.code?.toLowerCase().includes(term)
    );
  }

  get _filteredGateways() {
    if (!this._searchTerm) return this._gateways;
    const term = this._searchTerm.toLowerCase();
    return this._gateways.filter(g =>
      g.name?.toLowerCase().includes(term) ||
      g.code?.toLowerCase().includes(term)
    );
  }

  _navigateToMethod(method) {
    window.location.href = `/umbraco/section/ecommerce/workspace/ecommerce-payment-method/edit/${method.id}`;
  }

  _navigateToGateway(gateway) {
    window.location.href = `/umbraco/section/ecommerce/workspace/ecommerce-payment-gateway/edit/${gateway.id}`;
  }

  _createMethod() {
    window.location.href = '/umbraco/section/ecommerce/workspace/ecommerce-payment-method/create';
  }

  _createGateway() {
    window.location.href = '/umbraco/section/ecommerce/workspace/ecommerce-payment-gateway/create';
  }

  async _deleteMethod(method, e) {
    e.stopPropagation();
    if (!confirm(`Are you sure you want to delete "${method.name}"?`)) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/method/${method.id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        await this._loadMethods();
      }
    } catch (error) {
      console.error('Error deleting payment method:', error);
    }
  }

  async _deleteGateway(gateway, e) {
    e.stopPropagation();
    if (!confirm(`Are you sure you want to delete "${gateway.name}"?`)) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/gateway/${gateway.id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        await this._loadGateways();
      }
    } catch (error) {
      console.error('Error deleting payment gateway:', error);
    }
  }

  _getMethodTypeLabel(type) {
    const types = {
      'CreditCard': 'Credit Card',
      'DebitCard': 'Debit Card',
      'PayPal': 'PayPal',
      'BankTransfer': 'Bank Transfer',
      'DirectDebit': 'Direct Debit',
      'Cryptocurrency': 'Cryptocurrency',
      'BuyNowPayLater': 'Buy Now Pay Later',
      'DigitalWallet': 'Digital Wallet',
      'Cash': 'Cash on Delivery',
      'Check': 'Check',
      'Invoice': 'Invoice',
      'GiftCard': 'Gift Card',
      'StoreCredit': 'Store Credit'
    };
    return types[type] || type;
  }

  _getProviderLabel(type) {
    const types = {
      'Stripe': 'Stripe',
      'PayPal': 'PayPal',
      'Square': 'Square',
      'Braintree': 'Braintree',
      'AuthorizeNet': 'Authorize.Net',
      'Adyen': 'Adyen',
      'Mollie': 'Mollie',
      'Klarna': 'Klarna',
      'Manual': 'Manual/Offline'
    };
    return types[type] || type;
  }

  _renderMethodsTable() {
    if (this._filteredMethods.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-credit-card"></uui-icon>
          <h3>No Payment Methods</h3>
          <p>Create a payment method to accept payments.</p>
          <uui-button look="primary" @click=${this._createMethod}>
            <uui-icon name="icon-add"></uui-icon>
            Create Payment Method
          </uui-button>
        </div>
      `;
    }

    return html`
      <uui-table>
        <uui-table-head>
          <uui-table-head-cell>Name</uui-table-head-cell>
          <uui-table-head-cell>Code</uui-table-head-cell>
          <uui-table-head-cell>Type</uui-table-head-cell>
          <uui-table-head-cell>Status</uui-table-head-cell>
          <uui-table-head-cell>Order</uui-table-head-cell>
          <uui-table-head-cell></uui-table-head-cell>
        </uui-table-head>
        ${this._filteredMethods.map(method => html`
          <uui-table-row @click=${() => this._navigateToMethod(method)} style="cursor: pointer;">
            <uui-table-cell>${method.name}</uui-table-cell>
            <uui-table-cell><code>${method.code}</code></uui-table-cell>
            <uui-table-cell>${this._getMethodTypeLabel(method.type)}</uui-table-cell>
            <uui-table-cell>
              <span class="status-badge ${method.isActive ? 'status-active' : 'status-inactive'}">
                ${method.isActive ? 'Active' : 'Inactive'}
              </span>
            </uui-table-cell>
            <uui-table-cell>${method.sortOrder}</uui-table-cell>
            <uui-table-cell>
              <div class="actions">
                <uui-button look="secondary" compact @click=${(e) => this._deleteMethod(method, e)}>
                  <uui-icon name="icon-delete"></uui-icon>
                </uui-button>
              </div>
            </uui-table-cell>
          </uui-table-row>
        `)}
      </uui-table>
    `;
  }

  _renderGatewaysTable() {
    if (this._filteredGateways.length === 0) {
      return html`
        <div class="empty-state">
          <uui-icon name="icon-server"></uui-icon>
          <h3>No Payment Gateways</h3>
          <p>Configure a payment gateway to process payments.</p>
          <uui-button look="primary" @click=${this._createGateway}>
            <uui-icon name="icon-add"></uui-icon>
            Create Payment Gateway
          </uui-button>
        </div>
      `;
    }

    return html`
      <uui-table>
        <uui-table-head>
          <uui-table-head-cell>Name</uui-table-head-cell>
          <uui-table-head-cell>Code</uui-table-head-cell>
          <uui-table-head-cell>Provider</uui-table-head-cell>
          <uui-table-head-cell>Environment</uui-table-head-cell>
          <uui-table-head-cell>Status</uui-table-head-cell>
          <uui-table-head-cell></uui-table-head-cell>
        </uui-table-head>
        ${this._filteredGateways.map(gateway => html`
          <uui-table-row @click=${() => this._navigateToGateway(gateway)} style="cursor: pointer;">
            <uui-table-cell>${gateway.name}</uui-table-cell>
            <uui-table-cell><code>${gateway.code}</code></uui-table-cell>
            <uui-table-cell>${this._getProviderLabel(gateway.providerType)}</uui-table-cell>
            <uui-table-cell>
              <span class="status-badge ${gateway.isSandbox ? 'badge-sandbox' : 'badge-live'}">
                ${gateway.isSandbox ? 'Sandbox' : 'Live'}
              </span>
            </uui-table-cell>
            <uui-table-cell>
              <span class="status-badge ${gateway.isActive ? 'status-active' : 'status-inactive'}">
                ${gateway.isActive ? 'Active' : 'Inactive'}
              </span>
            </uui-table-cell>
            <uui-table-cell>
              <div class="actions">
                <uui-button look="secondary" compact @click=${(e) => this._deleteGateway(gateway, e)}>
                  <uui-icon name="icon-delete"></uui-icon>
                </uui-button>
              </div>
            </uui-table-cell>
          </uui-table-row>
        `)}
      </uui-table>
    `;
  }

  render() {
    if (this._loading) {
      return html`<uui-loader-bar></uui-loader-bar>`;
    }

    return html`
      <div class="tabs">
        <div
          class="tab ${this._activeTab === 'methods' ? 'active' : ''}"
          @click=${() => this._activeTab = 'methods'}
        >
          <uui-icon name="icon-credit-card"></uui-icon>
          Payment Methods (${this._methods.length})
        </div>
        <div
          class="tab ${this._activeTab === 'gateways' ? 'active' : ''}"
          @click=${() => this._activeTab = 'gateways'}
        >
          <uui-icon name="icon-server"></uui-icon>
          Payment Gateways (${this._gateways.length})
        </div>
      </div>

      <div class="toolbar">
        <uui-input
          class="search-box"
          placeholder="Search..."
          .value=${this._searchTerm}
          @input=${(e) => this._searchTerm = e.target.value}
        >
          <uui-icon name="icon-search" slot="prepend"></uui-icon>
        </uui-input>

        <uui-button
          look="primary"
          @click=${this._activeTab === 'methods' ? this._createMethod : this._createGateway}
        >
          <uui-icon name="icon-add"></uui-icon>
          Create ${this._activeTab === 'methods' ? 'Payment Method' : 'Payment Gateway'}
        </uui-button>
      </div>

      ${this._activeTab === 'methods' ? this._renderMethodsTable() : this._renderGatewaysTable()}
    `;
  }
}

customElements.define('ecommerce-payment-collection', PaymentCollection);

export default PaymentCollection;
