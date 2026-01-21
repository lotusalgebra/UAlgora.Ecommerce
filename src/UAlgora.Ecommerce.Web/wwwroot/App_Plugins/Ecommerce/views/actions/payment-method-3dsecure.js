import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method 3D Secure Action
 * Quick action to toggle 3D Secure requirement for payment methods.
 */
export class PaymentMethod3DSecureAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .security-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .enabled {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .disabled {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _require3DSecure: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._require3DSecure = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (workspace) {
      const method = workspace.getPaymentMethod();
      this._require3DSecure = method?.require3DSecure ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (!workspace) return;

    const method = workspace.getPaymentMethod();
    if (!method?.id) {
      alert('Please save the payment method first');
      return;
    }

    const newState = !this._require3DSecure;
    const action = newState ? 'enable' : 'disable';

    if (!confirm(`${action.charAt(0).toUpperCase() + action.slice(1)} 3D Secure for "${method.name}"?\n\n3D Secure adds an extra authentication step for card payments, reducing fraud but potentially increasing checkout friction.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/payment/method/${method.id}/toggle-3dsecure`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setPaymentMethod(result);
        this._require3DSecure = result.require3DSecure;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `3D Secure ${result.require3DSecure ? 'enabled' : 'disabled'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle 3D Secure');
      }
    } catch (error) {
      console.error('Error toggling 3D Secure:', error);
      alert('Failed to toggle 3D Secure');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._require3DSecure ? 'positive' : 'warning'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-shield"></uui-icon>
        3D Secure
        <span class="security-badge ${this._require3DSecure ? 'enabled' : 'disabled'}">
          ${this._require3DSecure ? 'On' : 'Off'}
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-method-3dsecure-action', PaymentMethod3DSecureAction);

export default PaymentMethod3DSecureAction;
