import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Toggle Action
 * Quick action to activate/deactivate a shipping method.
 */
export class ShippingMethodToggleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isActive: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isActive = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (workspace) {
      const method = workspace.getMethod();
      this._isActive = method?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (!workspace) return;

    const method = workspace.getMethod();
    if (!method?.id) {
      alert('Please save the shipping method first');
      return;
    }

    this._processing = true;

    try {
      // Get current method data
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/method/${method.id}`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error('Failed to load shipping method');
      }

      const currentMethod = await response.json();
      currentMethod.isActive = !currentMethod.isActive;

      // Update method
      const updateResponse = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/method/${method.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(currentMethod)
      });

      if (updateResponse.ok) {
        const result = await updateResponse.json();
        workspace.setMethod(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Shipping method activated' : 'Shipping method deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await updateResponse.json();
        alert(error.message || 'Failed to update shipping method');
      }
    } catch (error) {
      console.error('Error toggling shipping method:', error);
      alert('Failed to toggle shipping method');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isActive ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isActive ? 'icon-block' : 'icon-check'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isActive ? 'Deactivate' : 'Activate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-method-toggle-action', ShippingMethodToggleAction);

export default ShippingMethodToggleAction;
