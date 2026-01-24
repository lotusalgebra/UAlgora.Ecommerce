import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Marketing Toggle Action
 * Quick action to toggle customer marketing consent.
 */
export class CustomerMarketingAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _acceptsMarketing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._acceptsMarketing = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (workspace) {
      const customer = workspace.getCustomer();
      this._acceptsMarketing = customer?.acceptsMarketing ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (!workspace) return;

    const customer = workspace.getCustomer();
    if (!customer?.id) {
      alert('Please save the customer first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customers/${customer.id}/toggle-marketing`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCustomer(result);
        this._acceptsMarketing = result.acceptsMarketing;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.acceptsMarketing ? 'Customer opted in to marketing' : 'Customer opted out of marketing',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle marketing consent');
      }
    } catch (error) {
      console.error('Error toggling marketing consent:', error);
      alert('Failed to toggle marketing consent');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._acceptsMarketing ? 'positive' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._acceptsMarketing ? 'icon-message' : 'icon-block-message'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._acceptsMarketing ? 'Marketing: ON' : 'Marketing: OFF'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-customer-marketing-action', CustomerMarketingAction);

export default CustomerMarketingAction;
