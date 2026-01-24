import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Adjustment Complete Action
 * Completes the stock adjustment and applies changes.
 */
export class StockAdjustmentCompleteAction extends UmbElementMixin(LitElement) {
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

  async _complete() {
    const workspace = document.querySelector('ecommerce-stock-adjustment-workspace');
    if (!workspace) return;

    const adjustment = workspace.getAdjustment();

    if (adjustment.status !== 'Approved') {
      alert('Only approved adjustments can be completed');
      return;
    }

    if (!confirm('Are you sure you want to complete this stock adjustment? This will apply all stock changes and cannot be undone.')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/stock-adjustment/${adjustment.id}/complete`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setAdjustment(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to complete stock adjustment');
      }
    } catch (error) {
      console.error('Error completing stock adjustment:', error);
      alert('Failed to complete stock adjustment');
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
        @click=${this._complete}
      >
        ${this._processing ? 'Processing...' : 'Complete'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-adjustment-complete-action', StockAdjustmentCompleteAction);

export default StockAdjustmentCompleteAction;
