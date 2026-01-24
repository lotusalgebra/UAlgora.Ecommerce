import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Duplicate Action
 * Quick action to create a copy of the current shipping zone.
 */
export class ShippingZoneDuplicateAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-shipping-zone-workspace');
    if (!workspace) return;

    const zone = workspace.getZone();
    if (!zone?.id) {
      alert('Please save the shipping zone first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${zone.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/zone/${zone.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedZone = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Shipping zone duplicated: "${duplicatedZone.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the duplicated zone
        window.location.href = `/umbraco/section/ecommerce/workspace/shipping-zone/edit/${duplicatedZone.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate shipping zone');
      }
    } catch (error) {
      console.error('Error duplicating shipping zone:', error);
      alert('Failed to duplicate shipping zone');
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

customElements.define('ecommerce-shipping-zone-duplicate-action', ShippingZoneDuplicateAction);

export default ShippingZoneDuplicateAction;
