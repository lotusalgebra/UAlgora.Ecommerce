import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Reject Action
 * Quick action to reject a stock adjustment.
 */
export class StockAdjustmentRejectAction extends UmbElementMixin(LitElement) {
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

  async _handleReject() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getStockAdjustment();
    if (!adjustment?.id) {
      alert('Please save the stock adjustment first');
      return;
    }

    if (this._status !== 'Pending') {
      alert('Only pending adjustments can be rejected');
      return;
    }

    if (!confirm(`Reject adjustment "${adjustment.referenceNumber}"?\n\nThe adjustment will be marked as rejected.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-adjustment/${adjustment.id}/reject`, {
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
            headline: 'Rejected',
            message: 'Adjustment has been rejected',
            color: 'warning'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to reject adjustment');
      }
    } catch (error) {
      console.error('Error rejecting adjustment:', error);
      alert('Failed to reject adjustment');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const canReject = this._status === 'Pending';

    return html`
      <uui-button
        look="secondary"
        color="danger"
        ?disabled=${this._processing || !canReject}
        @click=${this._handleReject}
      >
        <uui-icon name="icon-block"></uui-icon>
        ${this._processing ? 'Rejecting...' : 'Reject'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-adjustment-reject-action', StockAdjustmentRejectAction);

export default StockAdjustmentRejectAction;
