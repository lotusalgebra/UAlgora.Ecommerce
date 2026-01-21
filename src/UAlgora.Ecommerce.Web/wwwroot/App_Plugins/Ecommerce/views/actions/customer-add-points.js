import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Add Points Action
 * Quick action to add loyalty points to a customer.
 */
export class CustomerAddPointsAction extends UmbElementMixin(LitElement) {
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

  async _addPoints() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (!workspace) return;

    const customer = workspace.getCustomer();
    if (!customer?.id) {
      alert('Please save the customer first');
      return;
    }

    const pointsStr = prompt('Enter loyalty points to add:');
    if (!pointsStr) return;

    const points = parseInt(pointsStr);
    if (isNaN(points) || points <= 0) {
      alert('Please enter a valid positive number');
      return;
    }

    const reason = prompt('Enter reason (optional):') || 'Manual adjustment';

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${customer.id}/loyalty-points`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          points: points,
          reason: reason
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCustomer(result);
        alert(`Added ${points.toLocaleString()} loyalty points successfully`);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to add loyalty points');
      }
    } catch (error) {
      console.error('Error adding loyalty points:', error);
      alert('Failed to add loyalty points');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._addPoints}
      >
        <uui-icon name="icon-medal"></uui-icon>
        ${this._processing ? 'Adding...' : 'Add Points'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-customer-add-points-action', CustomerAddPointsAction);

export default CustomerAddPointsAction;
