import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Submit Action
 * Quick action to submit a stock transfer for approval.
 */
export class StockTransferSubmitAction extends UmbElementMixin(LitElement) {
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

  async _handleSubmit() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getStockTransfer();
    if (!transfer?.id) {
      alert('Please save the stock transfer first');
      return;
    }

    if (this._status !== 'Draft') {
      alert('Only draft transfers can be submitted');
      return;
    }

    if (!confirm(`Submit transfer "${transfer.referenceNumber}" for approval?\n\nOnce submitted, the transfer cannot be modified.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-transfer/${transfer.id}/submit`, {
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
            message: 'Transfer submitted for approval',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to submit transfer');
      }
    } catch (error) {
      console.error('Error submitting transfer:', error);
      alert('Failed to submit transfer');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const canSubmit = this._status === 'Draft';

    return html`
      <uui-button
        look="primary"
        color="default"
        ?disabled=${this._processing || !canSubmit}
        @click=${this._handleSubmit}
      >
        <uui-icon name="icon-message"></uui-icon>
        ${this._processing ? 'Submitting...' : 'Submit for Approval'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-submit-action', StockTransferSubmitAction);

export default StockTransferSubmitAction;
