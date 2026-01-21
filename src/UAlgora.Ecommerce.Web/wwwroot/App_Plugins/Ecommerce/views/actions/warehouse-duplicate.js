import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Duplicate Action
 * Quick action to create a copy of the current warehouse.
 */
export class WarehouseDuplicateAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (!workspace) return;

    const warehouse = workspace.getWarehouse();
    if (!warehouse?.id) {
      alert('Please save the warehouse first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${warehouse.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedWarehouse = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Warehouse duplicated: "${duplicatedWarehouse.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the duplicated warehouse
        window.location.href = `/umbraco/section/ecommerce/workspace/warehouse/edit/${duplicatedWarehouse.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate warehouse');
      }
    } catch (error) {
      console.error('Error duplicating warehouse:', error);
      alert('Failed to duplicate warehouse');
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

customElements.define('ecommerce-warehouse-duplicate-action', WarehouseDuplicateAction);

export default WarehouseDuplicateAction;
