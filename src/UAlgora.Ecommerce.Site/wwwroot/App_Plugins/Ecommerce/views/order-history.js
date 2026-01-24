import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order History View
 * Displays order status history and timeline.
 */
export class OrderHistory extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .timeline {
      position: relative;
      padding-left: var(--uui-size-layout-2);
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
      padding-bottom: var(--uui-size-layout-1);
      padding-left: var(--uui-size-layout-1);
    }

    .timeline-item:last-child {
      padding-bottom: 0;
    }

    .timeline-dot {
      position: absolute;
      left: -26px;
      top: 4px;
      width: 12px;
      height: 12px;
      border-radius: 50%;
      background: var(--uui-color-border);
      border: 2px solid var(--uui-color-surface);
    }

    .timeline-dot.active {
      background: var(--uui-color-positive);
    }

    .timeline-dot.warning {
      background: var(--uui-color-warning);
    }

    .timeline-dot.danger {
      background: var(--uui-color-danger);
    }

    .timeline-content {
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-space-4);
    }

    .timeline-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-space-2);
    }

    .timeline-status {
      font-weight: bold;
      color: var(--uui-color-text);
    }

    .timeline-date {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .timeline-note {
      color: var(--uui-color-text-alt);
      font-size: var(--uui-type-small-size);
      margin-top: var(--uui-size-space-2);
      font-style: italic;
    }

    .status-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: var(--uui-size-space-4);
      margin-bottom: var(--uui-size-layout-1);
    }

    .status-step {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .status-step.completed {
      border-color: var(--uui-color-positive);
      background: var(--uui-color-positive-emphasis);
    }

    .status-step.current {
      border-color: var(--uui-color-warning);
      background: var(--uui-color-warning-emphasis);
    }

    .status-step uui-icon {
      font-size: 24px;
      margin-bottom: var(--uui-size-space-2);
    }

    .status-step.completed uui-icon {
      color: var(--uui-color-positive);
    }

    .status-step.current uui-icon {
      color: var(--uui-color-warning);
    }

    .status-step-label {
      font-size: var(--uui-type-small-size);
      font-weight: 500;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
    }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _history: { type: Array, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._order = null;
    this._history = [];
    this._loading = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-order-workspace');
    if (workspace) {
      this._order = workspace.getOrder();
      this._buildHistory();
    }
  }

  _buildHistory() {
    // Build a basic history from order data
    // In a real implementation, this would come from an API endpoint
    const history = [];

    if (this._order?.createdAt) {
      history.push({
        status: 'Created',
        date: this._order.createdAt,
        note: 'Order was placed',
        type: 'active'
      });
    }

    // Add status-based events
    const status = this._order?.status?.toLowerCase();
    if (status === 'confirmed' || status === 'processing' || status === 'shipped' || status === 'delivered' || status === 'completed') {
      history.push({
        status: 'Confirmed',
        date: this._order.updatedAt || this._order.createdAt,
        note: 'Order confirmed',
        type: 'active'
      });
    }

    if (status === 'processing' || status === 'shipped' || status === 'delivered' || status === 'completed') {
      history.push({
        status: 'Processing',
        date: this._order.updatedAt || this._order.createdAt,
        note: 'Order is being processed',
        type: 'active'
      });
    }

    if (status === 'shipped' || status === 'delivered' || status === 'completed') {
      history.push({
        status: 'Shipped',
        date: this._order.updatedAt || this._order.createdAt,
        note: 'Order has been shipped',
        type: 'active'
      });
    }

    if (status === 'delivered' || status === 'completed') {
      history.push({
        status: 'Delivered',
        date: this._order.updatedAt || this._order.createdAt,
        note: 'Order delivered',
        type: 'active'
      });
    }

    if (status === 'cancelled') {
      history.push({
        status: 'Cancelled',
        date: this._order.updatedAt || this._order.createdAt,
        note: 'Order was cancelled',
        type: 'danger'
      });
    }

    if (status === 'refunded') {
      history.push({
        status: 'Refunded',
        date: this._order.updatedAt || this._order.createdAt,
        note: 'Order was refunded',
        type: 'warning'
      });
    }

    this._history = history.reverse(); // Most recent first
  }

  _getStatusSteps() {
    const steps = [
      { key: 'pending', label: 'Pending', icon: 'icon-time' },
      { key: 'confirmed', label: 'Confirmed', icon: 'icon-check' },
      { key: 'processing', label: 'Processing', icon: 'icon-loading' },
      { key: 'shipped', label: 'Shipped', icon: 'icon-truck' },
      { key: 'delivered', label: 'Delivered', icon: 'icon-home' },
      { key: 'completed', label: 'Completed', icon: 'icon-checkbox-dotted' }
    ];

    const currentStatus = this._order?.status?.toLowerCase() || 'pending';
    const statusOrder = ['pending', 'confirmed', 'processing', 'shipped', 'delivered', 'completed'];
    const currentIndex = statusOrder.indexOf(currentStatus);

    // Handle cancelled/refunded separately
    if (currentStatus === 'cancelled' || currentStatus === 'refunded') {
      return steps.map(step => ({ ...step, state: 'pending' }));
    }

    return steps.map((step, index) => {
      const stepIndex = statusOrder.indexOf(step.key);
      if (stepIndex < currentIndex) {
        return { ...step, state: 'completed' };
      } else if (stepIndex === currentIndex) {
        return { ...step, state: 'current' };
      }
      return { ...step, state: 'pending' };
    });
  }

  render() {
    if (!this._order) {
      return html`<uui-loader></uui-loader>`;
    }

    const steps = this._getStatusSteps();
    const isCancelledOrRefunded = ['cancelled', 'refunded'].includes(this._order.status?.toLowerCase());

    return html`
      ${!isCancelledOrRefunded ? html`
        <uui-box>
          <div slot="headline">Order Progress</div>
          <div class="status-grid">
            ${steps.map(step => html`
              <div class="status-step ${step.state}">
                <uui-icon name="${step.icon}"></uui-icon>
                <span class="status-step-label">${step.label}</span>
              </div>
            `)}
          </div>
        </uui-box>
      ` : html`
        <uui-box>
          <div slot="headline" style="color: ${this._order.status === 'Cancelled' ? 'var(--uui-color-danger)' : 'var(--uui-color-warning)'}">
            Order ${this._order.status}
          </div>
          <p>This order has been ${this._order.status?.toLowerCase()}.</p>
        </uui-box>
      `}

      <h3 class="section-title">Activity Timeline</h3>
      <uui-box>
        <div class="timeline">
          ${this._history.length === 0 ? html`
            <p style="color: var(--uui-color-text-alt);">No activity recorded yet.</p>
          ` : this._history.map(item => html`
            <div class="timeline-item">
              <div class="timeline-dot ${item.type}"></div>
              <div class="timeline-content">
                <div class="timeline-header">
                  <span class="timeline-status">${item.status}</span>
                  <span class="timeline-date">${new Date(item.date).toLocaleString()}</span>
                </div>
                ${item.note ? html`<div class="timeline-note">${item.note}</div>` : ''}
              </div>
            </div>
          `)}
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-order-history', OrderHistory);

export default OrderHistory;
