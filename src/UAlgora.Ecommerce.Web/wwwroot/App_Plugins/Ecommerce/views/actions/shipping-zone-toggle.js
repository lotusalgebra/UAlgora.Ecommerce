import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Toggle Action
 * Quick action to activate/deactivate a shipping zone.
 */
export class ShippingZoneToggleAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-shipping-zone-workspace');
    if (workspace) {
      const zone = workspace.getZone();
      this._isActive = zone?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-shipping-zone-workspace');
    if (!workspace) return;

    const zone = workspace.getZone();
    if (!zone?.id) {
      alert('Please save the shipping zone first');
      return;
    }

    this._processing = true;

    try {
      // Get current zone data
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/zone/${zone.id}`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error('Failed to load shipping zone');
      }

      const currentZone = await response.json();
      currentZone.isActive = !currentZone.isActive;

      // Update zone
      const updateResponse = await fetch(`/umbraco/management/api/v1/ecommerce/shipping/zone/${zone.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(currentZone)
      });

      if (updateResponse.ok) {
        const result = await updateResponse.json();
        workspace.setZone(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Shipping zone activated' : 'Shipping zone deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await updateResponse.json();
        alert(error.message || 'Failed to update shipping zone');
      }
    } catch (error) {
      console.error('Error toggling shipping zone:', error);
      alert('Failed to toggle shipping zone');
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

customElements.define('ecommerce-shipping-zone-toggle-action', ShippingZoneToggleAction);

export default ShippingZoneToggleAction;
