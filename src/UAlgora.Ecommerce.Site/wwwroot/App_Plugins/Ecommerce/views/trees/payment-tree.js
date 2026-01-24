import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Tree
 * Tree structure for payment methods and gateways.
 */
export class PaymentTree extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
    }

    .tree-container {
      padding: var(--uui-size-space-3);
    }

    .tree-section {
      margin-bottom: var(--uui-size-space-4);
    }

    .section-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-2);
      cursor: pointer;
    }

    .section-header:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .section-title {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
      font-weight: 500;
    }

    .tree-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-space-2) var(--uui-size-space-3);
      padding-left: var(--uui-size-space-6);
      border-radius: var(--uui-border-radius);
      cursor: pointer;
    }

    .tree-item:hover {
      background: var(--uui-color-surface-alt);
    }

    .tree-item-info {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }

    .status-active {
      background: var(--uui-color-positive);
    }

    .status-inactive {
      background: var(--uui-color-text-alt);
    }

    .badge {
      font-size: 10px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-alt);
    }

    .badge-sandbox {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .empty-message {
      color: var(--uui-color-text-alt);
      font-style: italic;
      padding: var(--uui-size-space-2) var(--uui-size-space-6);
    }
  `;

  static properties = {
    _methods: { type: Array, state: true },
    _gateways: { type: Array, state: true },
    _methodsExpanded: { type: Boolean, state: true },
    _gatewaysExpanded: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._methods = [];
    this._gateways = [];
    this._methodsExpanded = true;
    this._gatewaysExpanded = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadData();
  }

  async _loadData() {
    await Promise.all([
      this._loadMethods(),
      this._loadGateways()
    ]);
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

  _getMethodTypeLabel(type) {
    const types = {
      'CreditCard': 'Credit Card',
      'DebitCard': 'Debit Card',
      'PayPal': 'PayPal',
      'BankTransfer': 'Bank Transfer',
      'DirectDebit': 'Direct Debit',
      'Cryptocurrency': 'Crypto',
      'BuyNowPayLater': 'BNPL',
      'DigitalWallet': 'Wallet',
      'Cash': 'Cash',
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
      'Manual': 'Manual'
    };
    return types[type] || type;
  }

  render() {
    return html`
      <div class="tree-container">
        <div class="tree-section">
          <div class="section-header" @click=${() => this._methodsExpanded = !this._methodsExpanded}>
            <span class="section-title">
              <uui-icon name="${this._methodsExpanded ? 'icon-navigation-down' : 'icon-navigation-right'}"></uui-icon>
              <uui-icon name="icon-credit-card"></uui-icon>
              Payment Methods
            </span>
            <uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._createMethod(); }}>
              <uui-icon name="icon-add"></uui-icon>
            </uui-button>
          </div>

          ${this._methodsExpanded ? html`
            ${this._methods.length === 0
              ? html`<div class="empty-message">No payment methods configured</div>`
              : this._methods.map(method => html`
                  <div class="tree-item" @click=${() => this._navigateToMethod(method)}>
                    <span class="tree-item-info">
                      <span class="status-dot ${method.isActive ? 'status-active' : 'status-inactive'}"></span>
                      ${method.name}
                    </span>
                    <span class="badge">${this._getMethodTypeLabel(method.type)}</span>
                  </div>
                `)
            }
          ` : ''}
        </div>

        <div class="tree-section">
          <div class="section-header" @click=${() => this._gatewaysExpanded = !this._gatewaysExpanded}>
            <span class="section-title">
              <uui-icon name="${this._gatewaysExpanded ? 'icon-navigation-down' : 'icon-navigation-right'}"></uui-icon>
              <uui-icon name="icon-server"></uui-icon>
              Payment Gateways
            </span>
            <uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._createGateway(); }}>
              <uui-icon name="icon-add"></uui-icon>
            </uui-button>
          </div>

          ${this._gatewaysExpanded ? html`
            ${this._gateways.length === 0
              ? html`<div class="empty-message">No payment gateways configured</div>`
              : this._gateways.map(gateway => html`
                  <div class="tree-item" @click=${() => this._navigateToGateway(gateway)}>
                    <span class="tree-item-info">
                      <span class="status-dot ${gateway.isActive ? 'status-active' : 'status-inactive'}"></span>
                      ${gateway.name}
                    </span>
                    <span class="badge ${gateway.isSandbox ? 'badge-sandbox' : ''}">${this._getProviderLabel(gateway.providerType)}</span>
                  </div>
                `)
            }
          ` : ''}
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-payment-tree', PaymentTree);

export default PaymentTree;
