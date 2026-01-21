import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Priority Action
 * Quick action to change warehouse fulfillment priority.
 */
export class WarehousePriorityAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .priority-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .high-priority {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _priority: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._priority = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (workspace) {
      const warehouse = workspace.getWarehouse();
      this._priority = warehouse?.priority ?? 0;
    }
  }

  async _handlePriority() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (!workspace) return;

    const warehouse = workspace.getWarehouse();
    if (!warehouse?.id) {
      alert('Please save the warehouse first');
      return;
    }

    const input = prompt(
      `Current priority: ${this._priority}\n` +
      `Enter new priority (higher numbers are checked first for fulfillment):`,
      String(this._priority)
    );
    if (input === null) return;

    const newPriority = parseInt(input, 10);
    if (isNaN(newPriority)) {
      alert('Please enter a valid number');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/warehouse/${warehouse.id}/update-priority`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ priority: newPriority })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setWarehouse(result);
        this._priority = result.priority;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Priority updated to ${newPriority}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update priority');
      }
    } catch (error) {
      console.error('Error updating priority:', error);
      alert('Failed to update priority');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const isHighPriority = this._priority >= 10;

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handlePriority}
      >
        <uui-icon name="icon-traffic"></uui-icon>
        Priority
        <span class="priority-badge ${isHighPriority ? 'high-priority' : ''}">${this._priority}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-warehouse-priority-action', WarehousePriorityAction);

export default WarehousePriorityAction;
