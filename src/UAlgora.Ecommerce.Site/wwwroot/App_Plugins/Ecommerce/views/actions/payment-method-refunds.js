import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Refunds Action
 * Quick action to toggle refunds for payment methods.
 */
export class PaymentMethodRefundsAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .refund-badge {
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
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _allowRefunds: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._allowRefunds = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (workspace) {
      const method = workspace.getPaymentMethod();
      this._allowRefunds = method?.allowRefunds ?? true;
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

    const newState = !this._allowRefunds;
    const action = newState ? 'enable' : 'disable';

    if (!confirm(`${action.charAt(0).toUpperCase() + action.slice(1)} refunds for "${method.name}"?\n\n${newState ? 'Customers will be able to receive refunds through this payment method.' : 'Refunds will not be allowed for orders using this payment method.'}`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/payment/method/${method.id}/toggle-refunds`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setPaymentMethod(result);
        this._allowRefunds = result.allowRefunds;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Refunds ${result.allowRefunds ? 'enabled' : 'disabled'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle refunds');
      }
    } catch (error) {
      console.error('Error toggling refunds:', error);
      alert('Failed to toggle refunds');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._allowRefunds ? 'positive' : 'danger'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-undo"></uui-icon>
        Refunds
        <span class="refund-badge ${this._allowRefunds ? 'enabled' : 'disabled'}">
          ${this._allowRefunds ? 'On' : 'Off'}
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-method-refunds-action', PaymentMethodRefundsAction);

export default PaymentMethodRefundsAction;
