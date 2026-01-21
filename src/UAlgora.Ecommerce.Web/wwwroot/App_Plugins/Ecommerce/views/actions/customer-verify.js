import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Verify Email Action
 * Quick action to mark customer email as verified.
 */
export class CustomerVerifyAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _emailVerified: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._emailVerified = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (workspace) {
      const customer = workspace.getCustomer();
      this._emailVerified = customer?.emailVerified ?? false;
    }
  }

  async _handleVerify() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (!workspace) return;

    const customer = workspace.getCustomer();
    if (!customer?.id) {
      alert('Please save the customer first');
      return;
    }

    if (this._emailVerified) {
      alert('Email is already verified');
      return;
    }

    const confirmed = confirm(`Mark ${customer.email} as verified?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customers/${customer.id}/verify-email`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCustomer(result);
        this._emailVerified = true;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Customer email verified',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to verify email');
      }
    } catch (error) {
      console.error('Error verifying email:', error);
      alert('Failed to verify email');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._emailVerified ? 'positive' : 'warning'}"
        ?disabled=${this._processing || this._emailVerified}
        @click=${this._handleVerify}
      >
        <uui-icon name="${this._emailVerified ? 'icon-check' : 'icon-message'}"></uui-icon>
        ${this._processing ? 'Verifying...' : this._emailVerified ? 'Email Verified' : 'Verify Email'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-customer-verify-action', CustomerVerifyAction);

export default CustomerVerifyAction;
