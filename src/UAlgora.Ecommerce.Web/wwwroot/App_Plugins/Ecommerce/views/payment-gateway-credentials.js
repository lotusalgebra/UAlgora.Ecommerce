import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Credentials
 * Editor for API credentials.
 */
export class PaymentGatewayCredentials extends UmbElementMixin(LitElement) {
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

    uui-input {
      width: 100%;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .warning-box {
      padding: var(--uui-size-space-4);
      background: var(--uui-color-warning-emphasis);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
    }

    .warning-box uui-icon {
      margin-right: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _gateway: { type: Object, state: true }
  };

  constructor() {
    super();
    this._gateway = {};
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-payment-gateway-workspace');
      if (workspace) {
        this._gateway = workspace.getGateway();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateGateway(field, value) {
    this._gateway = { ...this._gateway, [field]: value };
    const workspace = this.closest('ecommerce-payment-gateway-workspace');
    if (workspace) {
      workspace.setGateway(this._gateway);
    }
  }

  _renderStripeCredentials() {
    return html`
      <uui-box headline="Live Credentials">
        <div class="form-group">
          <label>Publishable Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.apiKey || ''}
            @input=${(e) => this._updateGateway('apiKey', e.target.value)}
            placeholder="pk_live_..."
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Secret Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.secretKey || ''}
            @input=${(e) => this._updateGateway('secretKey', e.target.value)}
            placeholder="sk_live_..."
          ></uui-input>
        </div>
      </uui-box>

      <uui-box headline="Sandbox Credentials">
        <div class="form-group">
          <label>Sandbox Publishable Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.sandboxApiKey || ''}
            @input=${(e) => this._updateGateway('sandboxApiKey', e.target.value)}
            placeholder="pk_test_..."
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Sandbox Secret Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.sandboxSecretKey || ''}
            @input=${(e) => this._updateGateway('sandboxSecretKey', e.target.value)}
            placeholder="sk_test_..."
          ></uui-input>
        </div>
      </uui-box>
    `;
  }

  _renderPayPalCredentials() {
    return html`
      <uui-box headline="API Credentials">
        <div class="form-group">
          <label>Client ID</label>
          <uui-input
            type="password"
            .value=${this._gateway.clientId || ''}
            @input=${(e) => this._updateGateway('clientId', e.target.value)}
            placeholder="PayPal Client ID"
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Client Secret</label>
          <uui-input
            type="password"
            .value=${this._gateway.clientSecret || ''}
            @input=${(e) => this._updateGateway('clientSecret', e.target.value)}
            placeholder="PayPal Client Secret"
          ></uui-input>
        </div>
      </uui-box>
    `;
  }

  _renderGenericCredentials() {
    return html`
      <uui-box headline="Live Credentials">
        <div class="form-group">
          <label>API Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.apiKey || ''}
            @input=${(e) => this._updateGateway('apiKey', e.target.value)}
            placeholder="API Key"
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Secret Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.secretKey || ''}
            @input=${(e) => this._updateGateway('secretKey', e.target.value)}
            placeholder="Secret Key"
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Merchant ID</label>
          <uui-input
            .value=${this._gateway.merchantId || ''}
            @input=${(e) => this._updateGateway('merchantId', e.target.value)}
            placeholder="Merchant ID (if required)"
          ></uui-input>
        </div>
      </uui-box>

      <uui-box headline="Sandbox Credentials">
        <div class="form-group">
          <label>Sandbox API Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.sandboxApiKey || ''}
            @input=${(e) => this._updateGateway('sandboxApiKey', e.target.value)}
            placeholder="Sandbox API Key"
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Sandbox Secret Key</label>
          <uui-input
            type="password"
            .value=${this._gateway.sandboxSecretKey || ''}
            @input=${(e) => this._updateGateway('sandboxSecretKey', e.target.value)}
            placeholder="Sandbox Secret Key"
          ></uui-input>
        </div>
        <div class="form-group">
          <label>Sandbox Merchant ID</label>
          <uui-input
            .value=${this._gateway.sandboxMerchantId || ''}
            @input=${(e) => this._updateGateway('sandboxMerchantId', e.target.value)}
            placeholder="Sandbox Merchant ID"
          ></uui-input>
        </div>
      </uui-box>
    `;
  }

  render() {
    if (this._gateway.providerType === 'Manual') {
      return html`
        <uui-box headline="Manual Payment">
          <p>This gateway type does not require API credentials. Payments will be processed manually.</p>
        </uui-box>
      `;
    }

    return html`
      <div class="warning-box">
        <uui-icon name="icon-lock"></uui-icon>
        <strong>Security Notice:</strong> API credentials are stored securely. Never share these credentials.
      </div>

      <div class="editor-grid">
        ${this._gateway.providerType === 'Stripe' ? this._renderStripeCredentials() : ''}
        ${this._gateway.providerType === 'PayPal' ? this._renderPayPalCredentials() : ''}
        ${!['Stripe', 'PayPal', 'Manual'].includes(this._gateway.providerType) ? this._renderGenericCredentials() : ''}
      </div>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-credentials', PaymentGatewayCredentials);

export default PaymentGatewayCredentials;
