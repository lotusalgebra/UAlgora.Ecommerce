import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Zone Save Action
 */
export class ShippingZoneSaveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: inline-block;
    }
  `;

  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _handleSave() {
    const workspace = this.closest('ecommerce-shipping-zone-workspace');
    if (!workspace) return;

    const zone = workspace.getZone();
    const isNew = workspace.isNewZone();

    if (!zone.name) {
      this._showNotification('warning', 'Please enter a zone name');
      return;
    }

    if (!zone.code) {
      this._showNotification('warning', 'Please enter a zone code');
      return;
    }

    try {
      this._saving = true;

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/shipping/zone'
        : `/umbraco/management/api/v1/ecommerce/shipping/zone/${zone.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(zone)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save shipping zone');
      }

      const savedZone = await response.json();
      workspace.setZone(savedZone);

      this._showNotification('positive', isNew ? 'Shipping zone created' : 'Shipping zone saved');

      if (isNew && savedZone.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/shipping-zone/edit/${savedZone.id}`);
      }
    } catch (error) {
      console.error('Error saving shipping zone:', error);
      this._showNotification('danger', error.message || 'Failed to save shipping zone');
    } finally {
      this._saving = false;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : color === 'warning' ? 'Warning' : 'Error',
        message,
        color
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._handleSave}
      >
        ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : ''}
        Save
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-zone-save-action', ShippingZoneSaveAction);

export default ShippingZoneSaveAction;
