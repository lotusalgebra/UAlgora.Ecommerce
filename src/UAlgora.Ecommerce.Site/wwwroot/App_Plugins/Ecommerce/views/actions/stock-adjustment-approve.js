import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Approve Action
 * Quick action to approve a stock adjustment.
 */
export class StockAdjustmentApproveAction extends UmbElementMixin(LitElement) {
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

  async _handleApprove() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getStockAdjustment();
    if (!adjustment?.id) {
      alert('Please save the stock adjustment first');
      return;
    }

    if (this._status !== 'Pending') {
      alert('Only pending adjustments can be approved');
      return;
    }

    if (!confirm(`Approve adjustment "${adjustment.referenceNumber}"?\n\nThe adjustment will be ready to apply to inventory.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-adjustment/${adjustment.id}/approve`, {
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
            headline: 'Success',
            message: 'Adjustment approved',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to approve adjustment');
      }
    } catch (error) {
      console.error('Error approving adjustment:', error);
      alert('Failed to approve adjustment');
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

customElements.define('ecommerce-stock-adjustment-approve-action', StockAdjustmentApproveAction);

export default StockAdjustmentApproveAction;
