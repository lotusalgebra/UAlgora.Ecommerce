import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Purchase Order Approve Action
 * Approves the purchase order.
 */
export class PurchaseOrderApproveAction extends UmbElementMixin(LitElement) {
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

  async _approve() {
    const workspace = document.querySelector('ecommerce-purchase-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();

    if (order.status !== 'Pending') {
      alert('Only pending orders can be approved');
      return;
    }

    if (!confirm('Are you sure you want to approve this purchase order?')) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/inventory/purchase-order/${order.id}/approve`, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to approve purchase order');
      }
    } catch (error) {
      console.error('Error approving purchase order:', error);
      alert('Failed to approve purchase order');
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
        @click=${this._approve}
      >
        ${this._processing ? 'Processing...' : 'Approve'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-purchase-order-approve-action', PurchaseOrderApproveAction);

export default PurchaseOrderApproveAction;
