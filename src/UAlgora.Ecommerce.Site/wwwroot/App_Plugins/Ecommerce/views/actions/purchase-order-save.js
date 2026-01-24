import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Purchase Order Save Action
 * Saves the purchase order to the server.
 */
export class PurchaseOrderSaveAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-purchase-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    const isNew = workspace.isNewOrder();

    if (!order.supplierId || !order.warehouseId) {
      alert('Please select a supplier and warehouse');
      return;
    }

    this._saving = true;

    try {
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/inventory/purchase-order'
        : `/umbraco/management/api/v1/ecommerce/inventory/purchase-order/${order.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(order)
      });

      if (response.ok) {
        const result = await response.json();
        if (isNew) {
          window.history.replaceState({}, '', `/umbraco/section/ecommerce/purchase-order/edit/${result.id}`);
        }
        workspace.setOrder(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save purchase order');
      }
    } catch (error) {
      console.error('Error saving purchase order:', error);
      alert('Failed to save purchase order');
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

customElements.define('ecommerce-purchase-order-save-action', PurchaseOrderSaveAction);

export default PurchaseOrderSaveAction;
