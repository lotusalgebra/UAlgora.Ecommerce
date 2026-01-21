import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Fulfillment Action
 * Quick action to toggle warehouse order fulfillment capability.
 */
export class WarehouseFulfillmentAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .status-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .enabled {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }

    .disabled {
      background: var(--uui-color-danger-emphasis);
      color: var(--uui-color-danger);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _canFulfillOrders: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._canFulfillOrders = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (workspace) {
      const warehouse = workspace.getWarehouse();
      this._canFulfillOrders = warehouse?.canFulfillOrders ?? true;
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

    const newState = !this._canFulfillOrders;
    const action = newState ? 'enable' : 'disable';

    if (!confirm(`${action.charAt(0).toUpperCase() + action.slice(1)} order fulfillment for "${warehouse.name}"?\n\n${newState ? 'This warehouse will be used for order fulfillment.' : 'This warehouse will NOT be used for order fulfillment.'}`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/warehouse/${warehouse.id}/toggle-fulfillment`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWarehouse(result);
        this._canFulfillOrders = result.canFulfillOrders;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Order fulfillment ${result.canFulfillOrders ? 'enabled' : 'disabled'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle fulfillment');
      }
    } catch (error) {
      console.error('Error toggling fulfillment:', error);
      alert('Failed to toggle fulfillment');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._canFulfillOrders ? 'positive' : 'danger'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-truck"></uui-icon>
        Fulfillment
        <span class="status-badge ${this._canFulfillOrders ? 'enabled' : 'disabled'}">
          ${this._canFulfillOrders ? 'On' : 'Off'}
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-warehouse-fulfillment-action', WarehouseFulfillmentAction);

export default WarehouseFulfillmentAction;
