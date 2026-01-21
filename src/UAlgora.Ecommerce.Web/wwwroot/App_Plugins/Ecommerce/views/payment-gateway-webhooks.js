import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Webhooks
 * Editor for webhook settings.
 */
export class PaymentGatewayWebhooks extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
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

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .webhook-url-box {
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
      word-break: break-all;
      font-family: monospace;
    }

    .copy-button {
      margin-top: var(--uui-size-space-2);
    }

    .info-box {
      padding: var(--uui-size-space-4);
      background: var(--uui-color-default-emphasis);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
    }

    .info-box uui-icon {
      margin-right: var(--uui-size-space-2);
    }
  `;

  static properties = {
    _gateway: { type: Object, state: true },
    _webhookUrl: { type: String, state: true }
  };

  constructor() {
    super();
    this._gateway = {};
    this._webhookUrl = '';
  }

  connectedCallback() {
    super.connectedCallback();
    this._generateWebhookUrl();

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

  _generateWebhookUrl() {
    const baseUrl = window.location.origin;
    this._webhookUrl = `${baseUrl}/umbraco/api/v1/ecommerce/webhook/payment`;
  }

  _updateGateway(field, value) {
    this._gateway = { ...this._gateway, [field]: value };
    const workspace = this.closest('ecommerce-payment-gateway-workspace');
    if (workspace) {
      workspace.setGateway(this._gateway);
    }
  }

  async _copyWebhookUrl() {
    try {
      await navigator.clipboard.writeText(this._webhookUrl);
      this._showNotification('positive', 'Webhook URL copied to clipboard');
    } catch (error) {
      this._showNotification('danger', 'Failed to copy URL');
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : 'Error',
        message,
        color
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    if (this._gateway.providerType === 'Manual') {
      return html`
        <uui-box headline="Webhooks">
          <p>Manual payment gateways do not use webhooks.</p>
        </uui-box>
      `;
    }

    return html`
      <uui-box headline="Webhook Configuration">
        <div class="info-box">
          <uui-icon name="icon-info"></uui-icon>
          Configure this URL in your payment provider's dashboard to receive payment notifications.
        </div>

        <div class="form-group">
          <label>Webhook Endpoint URL</label>
          <div class="webhook-url-box">
            ${this._webhookUrl}
          </div>
          <uui-button class="copy-button" look="secondary" @click=${this._copyWebhookUrl}>
            <uui-icon name="icon-paste"></uui-icon>
            Copy URL
          </uui-button>
        </div>

        <div class="form-group">
          <div class="checkbox-group">
            <uui-checkbox
              ?checked=${this._gateway.webhooksEnabled}
              @change=${(e) => this._updateGateway('webhooksEnabled', e.target.checked)}
            ></uui-checkbox>
            <label>Enable Webhooks</label>
          </div>
          <p class="help-text">When disabled, payment status must be verified manually</p>
        </div>
      </uui-box>

      <uui-box headline="Webhook Signing Secrets">
        <p class="help-text">Webhook secrets are used to verify that webhooks are sent from your payment provider.</p>

        <div class="form-group">
          <label>Live Webhook Secret</label>
          <uui-input
            type="password"
            .value=${this._gateway.webhookSecret || ''}
            @input=${(e) => this._updateGateway('webhookSecret', e.target.value)}
            placeholder="whsec_..."
          ></uui-input>
        </div>

        <div class="form-group">
          <label>Sandbox Webhook Secret</label>
          <uui-input
            type="password"
            .value=${this._gateway.sandboxWebhookSecret || ''}
            @input=${(e) => this._updateGateway('sandboxWebhookSecret', e.target.value)}
            placeholder="whsec_..."
          ></uui-input>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-webhooks', PaymentGatewayWebhooks);

export default PaymentGatewayWebhooks;
