import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Approve Action
 * Quick action to approve a stock transfer.
 */
export class StockTransferApproveAction extends UmbElementMixin(LitElement) {
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

  async _handleApprove() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getStockTransfer();
    if (!transfer?.id) {
      alert('Please save the stock transfer first');
      return;
    }

    if (this._status !== 'Pending') {
      alert('Only pending transfers can be approved');
      return;
    }

    if (!confirm(`Approve transfer "${transfer.referenceNumber}"?\n\nThe transfer will be ready to ship.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-transfer/${transfer.id}/approve`, {
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
            headline: 'Success',
            message: 'Transfer approved',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to approve transfer');
      }
    } catch (error) {
      console.error('Error approving transfer:', error);
      alert('Failed to approve transfer');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const canApprove = this._status === 'Pending';

    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._processing || !canApprove}
        @click=${this._handleApprove}
      >
        <uui-icon name="icon-check"></uui-icon>
        ${this._processing ? 'Approving...' : 'Approve'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-approve-action', StockTransferApproveAction);

export default StockTransferApproveAction;
