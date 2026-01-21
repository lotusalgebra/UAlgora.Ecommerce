import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Workspace
 * Main container for payment method editing.
 */
export class PaymentMethodWorkspace extends UmbElementMixin(LitElement) {
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

    .badge-default {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .badge-type {
      background: var(--uui-color-default-emphasis);
      color: var(--uui-color-default);
    }
  `;

  static properties = {
    _method: { type: Object, state: true },
    _isNew: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._method = {
      name: '',
      code: '',
      description: '',
      checkoutInstructions: '',
      type: 'CreditCard',
      gatewayId: null,
      isActive: true,
      isDefault: false,
      sortOrder: 0,
      feeType: 'None',
      flatFee: null,
      percentageFee: null,
      maxFee: null,
      showFeeAtCheckout: true,
      minOrderAmount: null,
      maxOrderAmount: null,
      allowedCountries: [],
      excludedCountries: [],
      allowedCurrencies: [],
      allowedCustomerGroups: [],
      iconName: null,
      imageUrl: null,
      showCardLogos: true,
      cssClass: null,
      captureMode: 'Immediate',
      autoCaptureDays: null,
      require3DSecure: false,
      requireCvv: true,
      requireBillingAddress: true,
      allowSavePaymentMethod: true,
      allowRefunds: true,
      allowPartialRefunds: true,
      refundTimeLimitDays: 0
    };
    this._isNew = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadMethod();
  }

  async _loadMethod() {
    const path = window.location.pathname;
    const editMatch = path.match(/\/payment-method\/edit\/([^/]+)/);

    if (editMatch) {
      this._isNew = false;
      const methodId = editMatch[1];

      try {
        const response = await fetch(`/umbraco/management/api/v1/ecommerce/payment/method/${methodId}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
          this._method = await response.json();
        }
      } catch (error) {
        console.error('Error loading payment method:', error);
      }
    }
  }

  getMethod() {
    return this._method;
  }

  setMethod(method) {
    this._method = { ...method };
    this.requestUpdate();
  }

  isNewMethod() {
    return this._isNew;
  }

  _getTypeLabel(type) {
    const types = {
      'CreditCard': 'Credit Card',
      'DebitCard': 'Debit Card',
      'PayPal': 'PayPal',
      'Stripe': 'Stripe',
      'BankTransfer': 'Bank Transfer',
      'CashOnDelivery': 'Cash on Delivery',
      'GiftCard': 'Gift Card',
      'StoreCredit': 'Store Credit',
      'Other': 'Other'
    };
    return types[type] || type;
  }

  render() {
    return html`
      <div class="workspace-header">
        <div class="header-info">
          <uui-icon name="icon-credit-card"></uui-icon>
          <div class="header-title">
            <h1>${this._isNew ? 'New Payment Method' : this._method.name || 'Payment Method'}</h1>
            <div class="header-meta">
              <span class="status-badge ${this._method.isActive ? 'status-active' : 'status-inactive'}">
                ${this._method.isActive ? 'Active' : 'Inactive'}
              </span>
              ${this._method.isDefault ? html`<span class="status-badge badge-default">Default</span>` : ''}
              <span class="status-badge badge-type">${this._getTypeLabel(this._method.type)}</span>
            </div>
          </div>
        </div>
        <slot name="action-menu"></slot>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-payment-method-workspace', PaymentMethodWorkspace);

export default PaymentMethodWorkspace;
