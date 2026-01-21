import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Warehouse Save Action
 * Saves the warehouse to the server.
 */
export class WarehouseSaveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _save() {
    const workspace = document.querySelector('ecommerce-warehouse-workspace');
    if (!workspace) return;

    const warehouse = workspace.getWarehouse();
    const isNew = workspace.isNewWarehouse();

    if (!warehouse.name || !warehouse.code) {
      alert('Please fill in all required fields');
      return;
    }

    this._saving = true;

    try {
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/inventory/warehouse'
        : `/umbraco/management/api/v1/ecommerce/inventory/warehouse/${warehouse.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(warehouse)
      });

      if (response.ok) {
        const result = await response.json();
        if (isNew) {
          window.history.replaceState({}, '', `/umbraco/section/ecommerce/warehouse/edit/${result.id}`);
        }
        workspace.setWarehouse(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save warehouse');
      }
    } catch (error) {
      console.error('Error saving warehouse:', error);
      alert('Failed to save warehouse');
    } finally {
      this._saving = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._save}
      >
        ${this._saving ? 'Saving...' : 'Save'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-warehouse-save-action', WarehouseSaveAction);

export default WarehouseSaveAction;
