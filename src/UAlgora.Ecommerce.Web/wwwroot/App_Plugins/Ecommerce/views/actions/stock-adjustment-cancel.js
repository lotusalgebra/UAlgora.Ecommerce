import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Cancel Action
 * Quick action to cancel a stock adjustment.
 */
export class StockAdjustmentCancelAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (workspace) {
      const adjustment = workspace.getStockAdjustment();
      this._status = adjustment?.status ?? 'Draft';
    }
  }

  async _handleCancel() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getStockAdjustment();
    if (!adjustment?.id) {
      alert('Please save the stock adjustment first');
      return;
    }

    const cancelableStatuses = ['Draft', 'Pending', 'Approved'];
    if (!cancelableStatuses.includes(this._status)) {
      alert('This adjustment cannot be cancelled');
      return;
    }

    if (!confirm(`Cancel adjustment "${adjustment.referenceNumber}"?\n\nThis action cannot be undone.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-adjustment/${adjustment.id}/cancel`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setStockAdjustment(result);
        this._status = result.status;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Cancelled',
            message: 'Adjustment has been cancelled',
            color: 'warning'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to cancel adjustment');
      }
    } catch (error) {
      console.error('Error cancelling adjustment:', error);
      alert('Failed to cancel adjustment');
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

customElements.define('ecommerce-stock-adjustment-cancel-action', StockAdjustmentCancelAction);

export default StockAdjustmentCancelAction;
