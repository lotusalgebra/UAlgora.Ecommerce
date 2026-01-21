import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Save Action
 * Saves the stock transfer to the server.
 */
export class StockTransferSaveAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getTransfer();
    const isNew = workspace.isNewTransfer();

    if (!transfer.sourceWarehouseId || !transfer.destinationWarehouseId) {
      alert('Please select source and destination warehouses');
      return;
    }

    if (transfer.sourceWarehouseId === transfer.destinationWarehouseId) {
      alert('Source and destination warehouses must be different');
      return;
    }

    this._saving = true;

    try {
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/inventory/stock-transfer'
        : `/umbraco/management/api/v1/ecommerce/inventory/stock-transfer/${transfer.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(transfer)
      });

      if (response.ok) {
        const result = await response.json();
        if (isNew) {
          window.history.replaceState({}, '', `/umbraco/section/ecommerce/stock-transfer/edit/${result.id}`);
        }
        workspace.setTransfer(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save stock transfer');
      }
    } catch (error) {
      console.error('Error saving stock transfer:', error);
      alert('Failed to save stock transfer');
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

customElements.define('ecommerce-stock-transfer-save-action', StockTransferSaveAction);

export default StockTransferSaveAction;
