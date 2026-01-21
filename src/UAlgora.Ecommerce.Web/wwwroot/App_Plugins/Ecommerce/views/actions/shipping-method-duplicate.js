import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Duplicate Action
 * Quick action to create a copy of the current shipping method.
 */
export class ShippingMethodDuplicateAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
  }

  async _handleDuplicate() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (!workspace) return;

    const method = workspace.getMethod();
    if (!method?.id) {
      alert('Please save the shipping method first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${method.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/method/${method.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedMethod = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Shipping method duplicated: "${duplicatedMethod.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the duplicated method
        window.location.href = `/umbraco/section/ecommerce/workspace/shipping-method/edit/${duplicatedMethod.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate shipping method');
      }
    } catch (error) {
      console.error('Error duplicating shipping method:', error);
      alert('Failed to duplicate shipping method');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-method-duplicate-action', ShippingMethodDuplicateAction);

export default ShippingMethodDuplicateAction;
