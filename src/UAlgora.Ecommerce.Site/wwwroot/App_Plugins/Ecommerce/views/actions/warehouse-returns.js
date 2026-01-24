import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Returns Action
 * Quick action to toggle warehouse returns acceptance.
 */
export class WarehouseReturnsAction extends UmbElementMixin(LitElement) {
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
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _acceptsReturns: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._acceptsReturns = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (workspace) {
      const warehouse = workspace.getWarehouse();
      this._acceptsReturns = warehouse?.acceptsReturns ?? true;
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

    const newState = !this._acceptsReturns;
    const action = newState ? 'enable' : 'disable';

    if (!confirm(`${action.charAt(0).toUpperCase() + action.slice(1)} returns for "${warehouse.name}"?\n\n${newState ? 'This warehouse will accept customer returns.' : 'This warehouse will NOT accept customer returns.'}`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/warehouse/${warehouse.id}/toggle-returns`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWarehouse(result);
        this._acceptsReturns = result.acceptsReturns;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Returns ${result.acceptsReturns ? 'enabled' : 'disabled'}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle returns');
      }
    } catch (error) {
      console.error('Error toggling returns:', error);
      alert('Failed to toggle returns');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._acceptsReturns ? 'positive' : 'warning'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-undo"></uui-icon>
        Returns
        <span class="status-badge ${this._acceptsReturns ? 'enabled' : 'disabled'}">
          ${this._acceptsReturns ? 'On' : 'Off'}
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-warehouse-returns-action', WarehouseReturnsAction);

export default WarehouseReturnsAction;
