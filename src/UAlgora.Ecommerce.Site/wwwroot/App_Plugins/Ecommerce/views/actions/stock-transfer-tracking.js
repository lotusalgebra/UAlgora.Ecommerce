import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Tracking Action
 * Quick action to update tracking information.
 */
export class StockTransferTrackingAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .tracking-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .has-tracking {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _trackingNumber: { type: String, state: true },
    _carrier: { type: String, state: true },
    _status: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._trackingNumber = null;
    this._carrier = null;
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
      this._trackingNumber = transfer?.trackingNumber ?? null;
      this._carrier = transfer?.carrier ?? null;
      this._status = transfer?.status ?? 'Draft';
    }
  }

  async _handleTracking() {
    const workspace = document.querySelector('ecommerce-stock-transfer-workspace');
    if (!workspace) return;

    const transfer = workspace.getStockTransfer();
    if (!transfer?.id) {
      alert('Please save the stock transfer first');
      return;
    }

    const trackingInput = prompt(
      `Current tracking number: ${this._trackingNumber || 'None'}\n` +
      `Enter tracking number:`,
      this._trackingNumber ?? ''
    );
    if (trackingInput === null) return;

    const carrierInput = prompt(
      `Current carrier: ${this._carrier || 'None'}\n` +
      `Enter carrier name (e.g., UPS, FedEx, DHL):`,
      this._carrier ?? ''
    );
    if (carrierInput === null) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/inventory/stock-transfer/${transfer.id}/update-tracking`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          trackingNumber: trackingInput || null,
          carrier: carrierInput || null
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setStockTransfer(result);
        this._trackingNumber = result.trackingNumber;
        this._carrier = result.carrier;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Tracking information updated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update tracking');
      }
    } catch (error) {
      console.error('Error updating tracking:', error);
      alert('Failed to update tracking');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const hasTracking = !!this._trackingNumber;
    const displayText = hasTracking ? this._trackingNumber : 'None';

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleTracking}
      >
        <uui-icon name="icon-truck"></uui-icon>
        Tracking
        <span class="tracking-badge ${hasTracking ? 'has-tracking' : ''}">${displayText}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-stock-transfer-tracking-action', StockTransferTrackingAction);

export default StockTransferTrackingAction;
