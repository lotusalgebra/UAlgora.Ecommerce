import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { UMB_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/workspace";

/**
 * Order Workspace Element
 * Main workspace container for viewing and managing orders in the Umbraco backoffice.
 */
export class OrderWorkspace extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }

    .order-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: var(--uui-size-layout-1);
      background: var(--uui-color-surface);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .order-number {
      font-size: var(--uui-type-h3-size);
      font-weight: bold;
    }

    .order-status {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .status-badge {
      padding: var(--uui-size-space-2) var(--uui-size-space-4);
      border-radius: var(--uui-border-radius);
      font-weight: 500;
      text-transform: uppercase;
      font-size: var(--uui-type-small-size);
    }

    .status-pending { background: #ffc107; color: #000; }
    .status-confirmed { background: #17a2b8; color: #fff; }
    .status-processing { background: #007bff; color: #fff; }
    .status-shipped { background: #6f42c1; color: #fff; }
    .status-delivered { background: #28a745; color: #fff; }
    .status-completed { background: #28a745; color: #fff; }
    .status-cancelled { background: #dc3545; color: #fff; }
    .status-refunded { background: #fd7e14; color: #fff; }
    .status-onhold { background: #6c757d; color: #fff; }
    .status-failed { background: #dc3545; color: #fff; }
  `;

  static properties = {
    _order: { type: Object, state: true },
    _loading: { type: Boolean, state: true },
    _error: { type: String, state: true }
  };

  #workspaceContext;

  constructor() {
    super();
    this._order = null;
    this._loading = true;
    this._error = null;
  }

  connectedCallback() {
    super.connectedCallback();

    this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
      this.#workspaceContext = context;
      this._loadOrder();
    });
  }

  async _loadOrder() {
    try {
      this._loading = true;

      const orderId = this.#workspaceContext?.getUnique();

      if (!orderId) {
        throw new Error('No order ID provided');
      }

      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${orderId}`, {
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error('Failed to load order');
      }

      this._order = await response.json();
      this._error = null;
    } catch (error) {
      this._error = error.message;
      console.error('Error loading order:', error);
    } finally {
      this._loading = false;
    }
  }

  getOrder() {
    return this._order;
  }

  setOrder(order) {
    this._order = { ...order };
    this.requestUpdate();
  }

  async refreshOrder() {
    await this._loadOrder();
  }

  isLoading() {
    return this._loading;
  }

  _getStatusClass(status) {
    return `status-${status?.toLowerCase() || 'pending'}`;
  }

  render() {
    if (this._loading) {
      return html`
        <div style="display: flex; justify-content: center; padding: var(--uui-size-layout-2);">
          <uui-loader></uui-loader>
        </div>
      `;
    }

    if (this._error) {
      return html`
        <uui-box>
          <div slot="headline">Error</div>
          <p>${this._error}</p>
          <uui-button @click=${this._loadOrder}>Retry</uui-button>
        </uui-box>
      `;
    }

    return html`
      <div class="order-header">
        <div>
          <div class="order-number">Order #${this._order?.orderNumber}</div>
          <div style="color: var(--uui-color-text-alt); font-size: var(--uui-type-small-size);">
            ${new Date(this._order?.createdAt).toLocaleString()}
          </div>
        </div>
        <div class="order-status">
          <span class="status-badge ${this._getStatusClass(this._order?.status)}">
            ${this._order?.status}
          </span>
        </div>
      </div>
      <slot></slot>
    `;
  }
}

customElements.define('ecommerce-order-workspace', OrderWorkspace);

export default OrderWorkspace;
