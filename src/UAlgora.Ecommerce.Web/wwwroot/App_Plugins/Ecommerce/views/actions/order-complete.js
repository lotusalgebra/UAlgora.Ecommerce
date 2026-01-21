import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Complete Action
 * Quick action to mark an order as completed.
 */
export class OrderCompleteAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _canComplete: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._canComplete = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (workspace) {
      const order = workspace.getOrder();
      const status = order?.status?.toLowerCase();
      // Can complete if order is delivered
      this._canComplete = order?.id && status === 'delivered';
    }
  }

  async _handleComplete() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    if (!confirm(`Mark order ${order.orderNumber} as completed?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/complete`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
        this._canComplete = false;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Order marked as completed',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to complete order');
      }
    } catch (error) {
      console.error('Error completing order:', error);
      alert('Failed to complete order');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (!this._canComplete) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="positive"
        ?disabled=${this._processing}
        @click=${this._handleComplete}
      >
        <uui-icon name="icon-checkbox-dotted"></uui-icon>
        ${this._processing ? 'Completing...' : 'Complete Order'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-complete-action', OrderCompleteAction);

export default OrderCompleteAction;
