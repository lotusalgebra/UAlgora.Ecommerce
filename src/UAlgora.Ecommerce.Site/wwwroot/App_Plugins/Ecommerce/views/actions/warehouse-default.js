import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Set Default Action
 * Quick action to set a warehouse as the default.
 */
export class WarehouseDefaultAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (workspace) {
      const warehouse = workspace.getWarehouse();
      this._isDefault = warehouse?.isDefault ?? false;
    }
  }

  async _handleSetDefault() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (!workspace) return;

    const warehouse = workspace.getWarehouse();
    if (!warehouse?.id) {
      alert('Please save the warehouse first');
      return;
    }

    if (this._isDefault) {
      alert('This warehouse is already the default');
      return;
    }

    const confirmed = confirm(`Set "${warehouse.name}" as the default warehouse? This will be used for stock allocation.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}/set-default`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        // Reload the warehouse to get updated data
        const reloadResponse = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (reloadResponse.ok) {
          const result = await reloadResponse.json();
          workspace.setWarehouse(result);
          this._isDefault = result.isDefault;
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Warehouse set as default',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to set default warehouse');
      }
    } catch (error) {
      console.error('Error setting default warehouse:', error);
      alert('Failed to set default warehouse');
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
        ${this._processing ? 'Setting...' : this._isDefault ? 'Default Warehouse' : 'Set as Default'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-warehouse-default-action', WarehouseDefaultAction);

export default WarehouseDefaultAction;
