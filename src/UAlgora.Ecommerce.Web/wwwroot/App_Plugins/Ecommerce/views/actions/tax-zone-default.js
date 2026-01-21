import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Zone Set Default Action
 * Quick action to set a tax zone as the default.
 */
export class TaxZoneDefaultAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isDefault: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isDefault = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-tax-zone-workspace');
    if (workspace) {
      const zone = workspace.getZone();
      this._isDefault = zone?.isDefault ?? false;
    }
  }

  async _handleSetDefault() {
    const workspace = document.querySelector('ecommerce-tax-zone-workspace');
    if (!workspace) return;

    const zone = workspace.getZone();
    if (!zone?.id) {
      alert('Please save the tax zone first');
      return;
    }

    if (this._isDefault) {
      alert('This zone is already the default');
      return;
    }

    const confirmed = confirm(`Set "${zone.name}" as the default tax zone? This will be used when no other zone matches.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/zone/${zone.id}/set-default`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        // Reload the zone to get updated data
        const reloadResponse = await fetch(`/umbraco/management/api/v1/ecommerce/tax/zone/${zone.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (reloadResponse.ok) {
          const result = await reloadResponse.json();
          workspace.setZone(result);
          this._isDefault = result.isDefault;
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Zone set as default',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to set default zone');
      }
    } catch (error) {
      console.error('Error setting default zone:', error);
      alert('Failed to set default zone');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isDefault ? 'default' : 'positive'}"
        ?disabled=${this._processing || this._isDefault}
        @click=${this._handleSetDefault}
      >
        <uui-icon name="${this._isDefault ? 'icon-check' : 'icon-favorite'}"></uui-icon>
        ${this._processing ? 'Setting...' : this._isDefault ? 'Default Zone' : 'Set as Default'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-tax-zone-default-action', TaxZoneDefaultAction);

export default TaxZoneDefaultAction;
