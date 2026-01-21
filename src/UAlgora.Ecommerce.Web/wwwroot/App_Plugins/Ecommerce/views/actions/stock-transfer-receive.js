import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Receive Action
 * Receives the stock transfer at the destination warehouse.
 */
export class StockTransferReceiveAction extends UmbElementMixin(LitElement) {
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

  async _receive() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getTransfer();

    if (transfer.status !== 'InTransit' && transfer.status !== 'PartiallyReceived') {
      alert('Only in-transit or partially received transfers can be received');
      return;
    }

    if (!confirm('Receive all items from this transfer?')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/stock-transfer/${transfer.id}/receive`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          receiveAll: true
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setTransfer(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to receive transfer');
      }
    } catch (error) {
      console.error('Error receiving transfer:', error);
      alert('Failed to receive transfer');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="positive"
        ?disabled=${this._processing}
        @click=${this._receive}
      >
        ${this._processing ? 'Processing...' : 'Receive'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-receive-action', StockTransferReceiveAction);

export default StockTransferReceiveAction;
