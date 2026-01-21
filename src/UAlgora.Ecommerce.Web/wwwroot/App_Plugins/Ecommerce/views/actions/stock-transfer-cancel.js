import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Cancel Action
 * Quick action to cancel a stock transfer.
 */
export class StockTransferCancelAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _status: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._status = 'Draft';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (workspace) {
      const transfer = workspace.getStockTransfer();
      this._status = transfer?.status ?? 'Draft';
    }
  }

  async _handleCancel() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getStockTransfer();
    if (!transfer?.id) {
      alert('Please save the stock transfer first');
      return;
    }

    const cancelableStatuses = ['Draft', 'Pending', 'Approved'];
    if (!cancelableStatuses.includes(this._status)) {
      alert('This transfer cannot be cancelled');
      return;
    }

    if (!confirm(`Cancel transfer "${transfer.referenceNumber}"?\n\nThis action cannot be undone.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-transfer/${transfer.id}/cancel`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setStockTransfer(result);
        this._status = result.status;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Cancelled',
            message: 'Transfer has been cancelled',
            color: 'warning'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to cancel transfer');
      }
    } catch (error) {
      console.error('Error cancelling transfer:', error);
      alert('Failed to cancel transfer');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const cancelableStatuses = ['Draft', 'Pending', 'Approved'];
    const canCancel = cancelableStatuses.includes(this._status);

    return html`
      <uui-button
        look="secondary"
        color="warning"
        ?disabled=${this._processing || !canCancel}
        @click=${this._handleCancel}
      >
        <uui-icon name="icon-wrong"></uui-icon>
        ${this._processing ? 'Cancelling...' : 'Cancel'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-cancel-action', StockTransferCancelAction);

export default StockTransferCancelAction;
