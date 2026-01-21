import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Save Action
 * Saves the stock adjustment to the server.
 */
export class StockAdjustmentSaveAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getAdjustment();
    const isNew = workspace.isNewAdjustment();

    if (!adjustment.warehouseId) {
      alert('Please select a warehouse');
      return;
    }

    this._saving = true;

    try {
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/inventory/stock-adjustment'
        : `/umbraco/management/api/v1/ecommerce/inventory/stock-adjustment/${adjustment.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(adjustment)
      });

      if (response.ok) {
        const result = await response.json();
        if (isNew) {
          window.history.replaceState({}, '', `/umbraco/section/ecommerce/stock-adjustment/edit/${result.id}`);
        }
        workspace.setAdjustment(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to save stock adjustment');
      }
    } catch (error) {
      console.error('Error saving stock adjustment:', error);
      alert('Failed to save stock adjustment');
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

customElements.define('ecommerce-stock-adjustment-save-action', StockAdjustmentSaveAction);

export default StockAdjustmentSaveAction;
