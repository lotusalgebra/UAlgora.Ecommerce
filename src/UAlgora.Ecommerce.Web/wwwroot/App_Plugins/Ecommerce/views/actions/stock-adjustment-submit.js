import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Submit Action
 * Quick action to submit a stock adjustment for approval.
 */
export class StockAdjustmentSubmitAction extends UmbElementMixin(LitElement) {
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

  async _handleSubmit() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getStockAdjustment();
    if (!adjustment?.id) {
      alert('Please save the stock adjustment first');
      return;
    }

    if (this._status !== 'Draft') {
      alert('Only draft adjustments can be submitted');
      return;
    }

    if (!confirm(`Submit adjustment "${adjustment.referenceNumber}" for approval?\n\nOnce submitted, the adjustment cannot be modified.`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-adjustment/${adjustment.id}/submit`, {
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
            message: 'Adjustment submitted for approval',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to submit adjustment');
      }
    } catch (error) {
      console.error('Error submitting adjustment:', error);
      alert('Failed to submit adjustment');
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

customElements.define('ecommerce-stock-adjustment-submit-action', StockAdjustmentSubmitAction);

export default StockAdjustmentSubmitAction;
