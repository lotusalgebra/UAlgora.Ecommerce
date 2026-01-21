import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Workspace
 * Main container for payment gateway editing.
 */
export class PaymentGatewayWorkspace extends UmbElementMixin(LitElement) {
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
      font-size: 32px;
      color: var(--uui-color-interactive);
    }

    .header-title h1 {
      margin: 0;
      font-size: var(--uui-type-h4-size);
    }

    .header-meta {
      display: flex;
      gap: var(--uui-size-space-3);
      margin-top: var(--uui-size-space-1);
    }

    .status-badge {
      display: inline-block;
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

    .badge-provider {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }
  `;

  static properties = {
    _gateway: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._gateway = {
      name: '',
      code: '',
      providerType: 'Stripe',
      description: '',
      isActive: true,
      isSandbox: true,
      sortOrder: 0,
      apiKey: null,
      secretKey: null,
      merchantId: null,
      clientId: null,
      clientSecret: null,
      sandboxApiKey: null,
      sandboxSecretKey: null,
      sandboxMerchantId: null,
      webhookUrl: null,
      webhookSecret: null,
      sandboxWebhookSecret: null,
      webhooksEnabled: true,
      supportedCurrencies: [],
      supportedCountries: [],
      supportedPaymentMethods: [],
      settings: {},
      statementDescriptor: null,
      statementDescriptorSuffix: null,
      brandName: null,
      landingPage: null,
      userAction: null
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadGateway();
  }

  async _loadGateway() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/payment-gateway\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const gatewayId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/gateway/${gatewayId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._gateway = await response.json();
        }
      } catch (error) {
        console.error('Error loading payment gateway:', error);
      }
    }
  }

  getGateway() {
    return this._gateway;
  }

  setGateway(gateway) {
    this._gateway = { ...gateway };
    this.requestUpdate();
  }

  isNewGateway() {
    return this._isNew;
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

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-server"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Payment Gateway' : this._gateway.name || 'Payment Gateway'}</h1>
            <div class="header-meta">
              <span class="status-badge ${this._gateway.isActive ? 'status-active' : 'status-inactive'}">
                ${this._gateway.isActive ? 'Active' : 'Inactive'}
              </span>
              <span class="status-badge ${this._gateway.isSandbox ? 'badge-sandbox' : 'badge-live'}">
                ${this._gateway.isSandbox ? 'Sandbox' : 'Live'}
              </span>
              <span class="status-badge badge-provider">${this._getProviderLabel(this._gateway.providerType)}</span>
            </div>
          </div>
        </div>
        <slot name="action-menu"></slot>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-workspace', PaymentGatewayWorkspace);

export default PaymentGatewayWorkspace;
