import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Stock Transfer Shipping
 * Shipping and tracking details for the stock transfer.
 */
export class StockTransferShipping extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .shipping-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--uui-size-layout-1);
    }

    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }

    .form-group {
      margin-bottom: var(--uui-size-space-5);
    }

    .form-group label {
      display: block;
      margin-bottom: var(--uui-size-space-2);
      font-weight: 500;
    }

    uui-input, uui-textarea, uui-select {
      width: 100%;
    }

    .help-text {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-top: var(--uui-size-space-1);
    }

    .timeline {
      position: relative;
      padding-left: var(--uui-size-space-6);
    }

    .timeline::before {
      content: '';
      position: absolute;
      left: 10px;
      top: 0;
      bottom: 0;
      width: 2px;
      background: var(--uui-color-border);
    }

    .timeline-item {
      position: relative;
      padding-bottom: var(--uui-size-space-5);
    }

    .timeline-item::before {
      content: '';
      position: absolute;
      left: -22px;
      top: 4px;
      width: 12px;
      height: 12px;
      border-radius: 50%;
      background: var(--uui-color-border);
    }

    .timeline-item.completed::before {
      background: var(--uui-color-positive);
    }

    .timeline-item.current::before {
      background: var(--uui-color-default);
      box-shadow: 0 0 0 4px var(--uui-color-default-emphasis);
    }

    .timeline-label {
      font-weight: 500;
      margin-bottom: var(--uui-size-space-1);
    }

    .timeline-date {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }
  `;

  static properties = {
    _transfer: { type: Object, state: true }
  };

  constructor() {
    super();
    this._transfer = {};
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-stock-transfer-workspace');
      if (workspace) {
        this._transfer = workspace.getTransfer();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateTransfer(field, value) {
    this._transfer = { ...this._transfer, [field]: value };
    const workspace = this.closest('ecommerce-stock-transfer-workspace');
    if (workspace) {
      workspace.setTransfer(this._transfer);
    }
  }

  _formatDate(date) {
    if (!date) return 'Pending';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  _getTimelineStatus(stage) {
    const statusOrder = ['Draft', 'Pending', 'Approved', 'InTransit', 'PartiallyReceived', 'Completed'];
    const currentIndex = statusOrder.indexOf(this._transfer.status);
    const stageIndex = statusOrder.indexOf(stage);

    if (stageIndex < currentIndex) return 'completed';
    if (stageIndex === currentIndex) return 'current';
    return 'pending';
  }

  render() {
    const canEdit = this._transfer.status === 'InTransit' ||
                    this._transfer.status === 'Approved' ||
                    this._transfer.status === 'Draft';

    return html`
      <div class="shipping-grid">
        <uui-box headline="Shipping Information">
          <div class="form-group">
            <label>Carrier</label>
            <uui-input
              .value=${this._transfer.carrier || ''}
              @input=${(e) => this._updateTransfer('carrier', e.target.value)}
              placeholder="e.g., FedEx, UPS, Internal"
              ?disabled=${!canEdit}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Tracking Number</label>
            <uui-input
              .value=${this._transfer.trackingNumber || ''}
              @input=${(e) => this._updateTransfer('trackingNumber', e.target.value)}
              placeholder="Enter tracking number"
              ?disabled=${!canEdit}
            ></uui-input>
            ${this._transfer.trackingNumber ? html`
              <p class="help-text">
                <a href="#" @click=${this._trackShipment}>Track Shipment</a>
              </p>
            ` : ''}
          </div>

          <div class="form-group">
            <label>Shipped Date</label>
            <uui-input
              type="datetime-local"
              .value=${this._transfer.shippedDate?.slice(0, 16) || ''}
              @input=${(e) => this._updateTransfer('shippedDate', e.target.value)}
              ?disabled=${!canEdit}
            ></uui-input>
          </div>

          <div class="form-group">
            <label>Received Date</label>
            <uui-input
              type="datetime-local"
              .value=${this._transfer.receivedDate?.slice(0, 16) || ''}
              @input=${(e) => this._updateTransfer('receivedDate', e.target.value)}
              ?disabled=${this._transfer.status !== 'InTransit' && this._transfer.status !== 'PartiallyReceived'}
            ></uui-input>
          </div>
        </uui-box>

        <uui-box headline="Transfer Timeline">
          <div class="timeline">
            <div class="timeline-item ${this._getTimelineStatus('Draft')}">
              <div class="timeline-label">Created</div>
              <div class="timeline-date">${this._formatDate(this._transfer.createdAt)}</div>
            </div>

            <div class="timeline-item ${this._getTimelineStatus('Approved')}">
              <div class="timeline-label">Approved</div>
              <div class="timeline-date">${this._formatDate(this._transfer.approvedAt)}</div>
            </div>

            <div class="timeline-item ${this._getTimelineStatus('InTransit')}">
              <div class="timeline-label">Shipped</div>
              <div class="timeline-date">${this._formatDate(this._transfer.shippedDate)}</div>
            </div>

            <div class="timeline-item ${this._getTimelineStatus('Completed')}">
              <div class="timeline-label">Received</div>
              <div class="timeline-date">${this._formatDate(this._transfer.receivedDate)}</div>
            </div>
          </div>
        </uui-box>
      </div>
    `;
  }

  _trackShipment(e) {
    e.preventDefault();
    // In a real implementation, this would open the carrier's tracking page
    alert('Tracking integration would open here');
  }
}

customElements.define('ecommerce-stock-transfer-shipping', StockTransferShipping);

export default StockTransferShipping;
