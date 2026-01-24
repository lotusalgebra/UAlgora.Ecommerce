import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Zone Save Action
 */
export class TaxZoneSaveAction extends UmbElementMixin(LitElement) {
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
    const workspace = this.closest('ecommerce-tax-zone-workspace');
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
        ? '/umbraco/management/api/v1/ecommerce/tax/zone'
        : `/umbraco/management/api/v1/ecommerce/tax/zone/${zone.id}`;

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
        throw new Error(errorData.message || 'Failed to save tax zone');
      }

      const savedZone = await response.json();
      workspace.setZone(savedZone);

      this._showNotification('positive', isNew ? 'Tax zone created' : 'Tax zone saved');

      if (isNew && savedZone.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/tax-zone/edit/${savedZone.id}`);
      }
    } catch (error) {
      console.error('Error saving tax zone:', error);
      this._showNotification('danger', error.message || 'Failed to save tax zone');
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

customElements.define('ecommerce-tax-zone-save-action', TaxZoneSaveAction);

export default TaxZoneSaveAction;
