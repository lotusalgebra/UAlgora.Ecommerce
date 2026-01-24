import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Toggle Action
 * Quick action to activate/deactivate a warehouse.
 */
export class WarehouseToggleAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (workspace) {
      const warehouse = workspace.getWarehouse();
      this._isActive = warehouse?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (!workspace) return;

    const warehouse = workspace.getWarehouse();
    if (!warehouse?.id) {
      alert('Please save the warehouse first');
      return;
    }

    this._processing = true;

    try {
      // Get current warehouse data
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error('Failed to load warehouse');
      }

      const currentWarehouse = await response.json();
      currentWarehouse.isActive = !currentWarehouse.isActive;

      // Update warehouse
      const updateResponse = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(currentWarehouse)
      });

      if (updateResponse.ok) {
        const result = await updateResponse.json();
        workspace.setWarehouse(result);
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Warehouse activated' : 'Warehouse deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await updateResponse.json();
        alert(error.message || 'Failed to update warehouse');
      }
    } catch (error) {
      console.error('Error toggling warehouse:', error);
      alert('Failed to toggle warehouse');
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

customElements.define('ecommerce-warehouse-toggle-action', WarehouseToggleAction);

export default WarehouseToggleAction;
