import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Ship Action
 * Marks the stock transfer as shipped.
 */
export class StockTransferShipAction extends UmbElementMixin(LitElement) {
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

  async _ship() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getTransfer();

    if (transfer.status !== 'Approved') {
      alert('Only approved transfers can be marked as shipped');
      return;
    }

    if (!confirm('Mark this transfer as shipped?')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/stock-transfer/${transfer.id}/ship`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          trackingNumber: transfer.trackingNumber,
          carrier: transfer.carrier
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setTransfer(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to mark as shipped');
      }
    } catch (error) {
      console.error('Error shipping transfer:', error);
      alert('Failed to mark as shipped');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._ship}
      >
        ${this._processing ? 'Processing...' : 'Mark Shipped'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-ship-action', StockTransferShipAction);

export default StockTransferShipAction;
