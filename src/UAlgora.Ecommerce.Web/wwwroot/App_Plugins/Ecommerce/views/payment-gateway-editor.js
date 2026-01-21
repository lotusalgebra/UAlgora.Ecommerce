import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Gateway Editor
 * Form for editing payment gateway basic settings.
 */
export class PaymentGatewayEditor extends UmbElementMixin(LitElement) {
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

    uui-input, uui-textarea, uui-select {
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

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Basic Information">
          <div class="form-group">
            <label>Name *</label>
            <uui-input
              .value=${this._gateway.name || ''}
              @input=${(e) => this._updateGateway('name', e.target.value)}
              placeholder="e.g., Stripe Production"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Code *</label>
            <uui-input
              .value=${this._gateway.code || ''}
              @input=${(e) => this._updateGateway('code', e.target.value)}
              placeholder="e.g., stripe-main"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Description</label>
            <uui-textarea
              .value=${this._gateway.description || ''}
              @input=${(e) => this._updateGateway('description', e.target.value)}
              placeholder="Internal description"
            ></uui-textarea>
          </div>
        </uui-box>

        <uui-box headline="Provider Settings">
          <div class="form-group">
            <label>Provider Type</label>
            <uui-select
              .value=${this._gateway.providerType || 'Stripe'}
              @change=${(e) => this._updateGateway('providerType', e.target.value)}
            >
              <option value="Stripe">Stripe</option>
              <option value="PayPal">PayPal</option>
              <option value="Square">Square</option>
              <option value="Braintree">Braintree</option>
              <option value="AuthorizeNet">Authorize.Net</option>
              <option value="Adyen">Adyen</option>
              <option value="Mollie">Mollie</option>
              <option value="Klarna">Klarna</option>
              <option value="Manual">Manual/Offline</option>
            </uui-select>
          </div>

          <div class="form-group">
            <label>Display Order</label>
            <uui-input
              type="number"
              .value=${this._gateway.sortOrder || 0}
              @input=${(e) => this._updateGateway('sortOrder', parseInt(e.target.value) || 0)}
            ></uui-input>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._gateway.isActive}
                @change=${(e) => this._updateGateway('isActive', e.target.checked)}
              ></uui-checkbox>
              <label>Active</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._gateway.isSandbox}
                @change=${(e) => this._updateGateway('isSandbox', e.target.checked)}
              ></uui-checkbox>
              <label>Sandbox/Test Mode</label>
            </div>
            <p class="help-text">When enabled, uses sandbox credentials for testing</p>
          </div>
        </uui-box>

        ${this._gateway.providerType === 'Stripe' ? html`
          <uui-box headline="Stripe Settings">
            <div class="form-group">
              <label>Statement Descriptor</label>
              <uui-input
                .value=${this._gateway.statementDescriptor || ''}
                @input=${(e) => this._updateGateway('statementDescriptor', e.target.value)}
                placeholder="Appears on customer statements"
                maxlength="22"
              ></uui-input>
              <p class="help-text">Max 22 characters</p>
            </div>

            <div class="form-group">
              <label>Statement Descriptor Suffix</label>
              <uui-input
                .value=${this._gateway.statementDescriptorSuffix || ''}
                @input=${(e) => this._updateGateway('statementDescriptorSuffix', e.target.value)}
                placeholder="Additional descriptor info"
                maxlength="22"
              ></uui-input>
            </div>
          </uui-box>
        ` : ''}

        ${this._gateway.providerType === 'PayPal' ? html`
          <uui-box headline="PayPal Settings">
            <div class="form-group">
              <label>Brand Name</label>
              <uui-input
                .value=${this._gateway.brandName || ''}
                @input=${(e) => this._updateGateway('brandName', e.target.value)}
                placeholder="Brand name shown in PayPal"
                maxlength="127"
              ></uui-input>
            </div>

            <div class="form-group">
              <label>Landing Page</label>
              <uui-select
                .value=${this._gateway.landingPage || 'LOGIN'}
                @change=${(e) => this._updateGateway('landingPage', e.target.value)}
              >
                <option value="LOGIN">Login (PayPal account)</option>
                <option value="BILLING">Billing (Guest checkout)</option>
                <option value="NO_PREFERENCE">No Preference</option>
              </uui-select>
            </div>

            <div class="form-group">
              <label>User Action</label>
              <uui-select
                .value=${this._gateway.userAction || 'PAY_NOW'}
                @change=${(e) => this._updateGateway('userAction', e.target.value)}
              >
                <option value="PAY_NOW">Pay Now</option>
                <option value="CONTINUE">Continue</option>
              </uui-select>
            </div>
          </uui-box>
        ` : ''}
      </div>
    `;
  }
}

customElements.define('ecommerce-payment-gateway-editor', PaymentGatewayEditor);

export default PaymentGatewayEditor;
