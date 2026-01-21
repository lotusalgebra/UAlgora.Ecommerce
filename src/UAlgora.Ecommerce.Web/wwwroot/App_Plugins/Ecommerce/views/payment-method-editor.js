import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Editor
 * Form for editing payment method details.
 */
export class PaymentMethodEditor extends UmbElementMixin(LitElement) {
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

    .full-width {
      grid-column: 1 / -1;
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

    .form-row {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-space-4);
    }

    uui-input, uui-textarea, uui-select {
      width: 100%;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .section-title {
      font-size: var(--uui-type-h5-size);
      font-weight: bold;
      margin-bottom: var(--uui-size-space-4);
      color: var(--uui-color-text);
    }
  `;

  static properties = {
    _method: { type: Object, state: true },
    _gateways: { type: Array, state: true }
  };

  constructor() {
    super();
    this._method = {};
    this._gateways = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadGateways();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-payment-method-workspace');
      if (workspace) {
        this._method = workspace.getMethod();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  async _loadGateways() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/payment/gateway?includeInactive=true', {
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        const data = await response.json();
        this._gateways = data.items || [];
      }
    } catch (error) {
      console.error('Error loading gateways:', error);
    }
  }

  _updateMethod(field, value) {
    this._method = { ...this._method, [field]: value };
    const workspace = this.closest('ecommerce-payment-method-workspace');
    if (workspace) {
      workspace.setMethod(this._method);
    }
  }

  render() {
    return html`
      <div class="editor-grid">
        <uui-box headline="Basic Information">
          <div class="form-group">
            <label>Name *</label>
            <uui-input
              .value=${this._method.name || ''}
              @input=${(e) => this._updateMethod('name', e.target.value)}
              placeholder="e.g., Credit Card"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Code *</label>
            <uui-input
              .value=${this._method.code || ''}
              @input=${(e) => this._updateMethod('code', e.target.value)}
              placeholder="e.g., credit-card"
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Description</label>
            <uui-textarea
              .value=${this._method.description || ''}
              @input=${(e) => this._updateMethod('description', e.target.value)}
              placeholder="Description shown to customers"
            ></uui-textarea>
          </div>

          <div class="form-group">
            <label>Checkout Instructions</label>
            <uui-textarea
              .value=${this._method.checkoutInstructions || ''}
              @input=${(e) => this._updateMethod('checkoutInstructions', e.target.value)}
              placeholder="Instructions displayed during checkout"
            ></uui-textarea>
          </div>
        </uui-box>

        <uui-box headline="Configuration">
          <div class="form-group">
            <label>Payment Type</label>
            <uui-select
              .value=${this._method.type || 'CreditCard'}
              @change=${(e) => this._updateMethod('type', e.target.value)}
            >
              <option value="CreditCard">Credit Card</option>
              <option value="DebitCard">Debit Card</option>
              <option value="PayPal">PayPal</option>
              <option value="Stripe">Stripe</option>
              <option value="BankTransfer">Bank Transfer</option>
              <option value="CashOnDelivery">Cash on Delivery</option>
              <option value="GiftCard">Gift Card</option>
              <option value="StoreCredit">Store Credit</option>
              <option value="Other">Other</option>
            </uui-select>
          </div>

          <div class="form-group">
            <label>Payment Gateway</label>
            <uui-select
              .value=${this._method.gatewayId || ''}
              @change=${(e) => this._updateMethod('gatewayId', e.target.value || null)}
            >
              <option value="">No gateway (offline)</option>
              ${this._gateways.map(g => html`
                <option value="${g.id}" ?selected=${this._method.gatewayId === g.id}>
                  ${g.name} (${g.providerType})
                </option>
              `)}
            </uui-select>
          </div>

          <div class="form-group">
            <label>Display Order</label>
            <uui-input
              type="number"
              .value=${this._method.sortOrder || 0}
              @input=${(e) => this._updateMethod('sortOrder', parseInt(e.target.value) || 0)}
            ></uui-input>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.isActive}
                @change=${(e) => this._updateMethod('isActive', e.target.checked)}
              ></uui-checkbox>
              <label>Active</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.isDefault}
                @change=${(e) => this._updateMethod('isDefault', e.target.checked)}
              ></uui-checkbox>
              <label>Default payment method</label>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Capture Settings">
          <div class="form-group">
            <label>Capture Mode</label>
            <uui-select
              .value=${this._method.captureMode || 'Immediate'}
              @change=${(e) => this._updateMethod('captureMode', e.target.value)}
            >
              <option value="Immediate">Capture Immediately</option>
              <option value="Manual">Manual Capture</option>
              <option value="Delayed">Delayed Capture</option>
            </uui-select>
          </div>

          ${this._method.captureMode === 'Delayed' ? html`
            <div class="form-group">
              <label>Auto-Capture Days</label>
              <uui-input
                type="number"
                .value=${this._method.autoCaptureDays || ''}
                @input=${(e) => this._updateMethod('autoCaptureDays', parseInt(e.target.value) || null)}
                placeholder="Days until auto-capture"
              ></uui-input>
            </div>
          ` : ''}
        </uui-box>

        <uui-box headline="Security Settings">
          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.require3DSecure}
                @change=${(e) => this._updateMethod('require3DSecure', e.target.checked)}
              ></uui-checkbox>
              <label>Require 3D Secure</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.requireCvv}
                @change=${(e) => this._updateMethod('requireCvv', e.target.checked)}
              ></uui-checkbox>
              <label>Require CVV</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.requireBillingAddress}
                @change=${(e) => this._updateMethod('requireBillingAddress', e.target.checked)}
              ></uui-checkbox>
              <label>Require Billing Address</label>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.allowSavePaymentMethod}
                @change=${(e) => this._updateMethod('allowSavePaymentMethod', e.target.checked)}
              ></uui-checkbox>
              <label>Allow customers to save payment method</label>
            </div>
          </div>
        </uui-box>

        <uui-box headline="Refund Settings" class="full-width">
          <div class="form-row">
            <div class="form-group">
              <div class="checkbox-group">
                <uui-checkbox
                  ?checked=${this._method.allowRefunds}
                  @change=${(e) => this._updateMethod('allowRefunds', e.target.checked)}
                ></uui-checkbox>
                <label>Allow Refunds</label>
              </div>
            </div>

            <div class="form-group">
              <div class="checkbox-group">
                <uui-checkbox
                  ?checked=${this._method.allowPartialRefunds}
                  @change=${(e) => this._updateMethod('allowPartialRefunds', e.target.checked)}
                ></uui-checkbox>
                <label>Allow Partial Refunds</label>
              </div>
            </div>
          </div>

          <div class="form-group">
            <label>Refund Time Limit (days, 0 = no limit)</label>
            <uui-input
              type="number"
              .value=${this._method.refundTimeLimitDays || 0}
              @input=${(e) => this._updateMethod('refundTimeLimitDays', parseInt(e.target.value) || 0)}
            ></uui-input>
          </div>
        </uui-box>

        <uui-box headline="Display Settings" class="full-width">
          <div class="form-row">
            <div class="form-group">
              <label>Icon Name</label>
              <uui-input
                .value=${this._method.iconName || ''}
                @input=${(e) => this._updateMethod('iconName', e.target.value)}
                placeholder="e.g., icon-credit-card"
              ></uui-input>
            </div>

            <div class="form-group">
              <label>Image URL</label>
              <uui-input
                .value=${this._method.imageUrl || ''}
                @input=${(e) => this._updateMethod('imageUrl', e.target.value)}
                placeholder="URL to payment method logo"
              ></uui-input>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <div class="checkbox-group">
                <uui-checkbox
                  ?checked=${this._method.showCardLogos}
                  @change=${(e) => this._updateMethod('showCardLogos', e.target.checked)}
                ></uui-checkbox>
                <label>Show Card Logos</label>
              </div>
            </div>

            <div class="form-group">
              <label>CSS Class</label>
              <uui-input
                .value=${this._method.cssClass || ''}
                @input=${(e) => this._updateMethod('cssClass', e.target.value)}
                placeholder="Custom CSS class"
              ></uui-input>
            </div>
          </div>
        </uui-box>
      </div>
    `;
  }
}

customElements.define('ecommerce-payment-method-editor', PaymentMethodEditor);

export default PaymentMethodEditor;
