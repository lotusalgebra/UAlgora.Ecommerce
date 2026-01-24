import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Customer Add Credit Action
 * Quick action to add store credit to a customer.
 */
export class CustomerAddCreditAction extends UmbElementMixin(LitElement) {
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

  async _addCredit() {
    const workspace = document.querySelector('ecommerce-customer-workspace');
    if (!workspace) return;

    const customer = workspace.getCustomer();
    if (!customer?.id) {
      alert('Please save the customer first');
      return;
    }

    const amountStr = prompt('Enter store credit amount to add:');
    if (!amountStr) return;

    const amount = parseFloat(amountStr);
    if (isNaN(amount) || amount <= 0) {
      alert('Please enter a valid positive amount');
      return;
    }

    const reason = prompt('Enter reason (optional):') || 'Manual adjustment';

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/customer/${customer.id}/store-credit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          amount: amount,
          reason: reason
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCustomer(result);
        alert(`Added $${amount.toFixed(2)} store credit successfully`);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to add store credit');
      }
    } catch (error) {
      console.error('Error adding store credit:', error);
      alert('Failed to add store credit');
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
        @click=${this._addCredit}
      >
        <uui-icon name="icon-coin-dollar"></uui-icon>
        ${this._processing ? 'Adding...' : 'Add Credit'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-customer-add-credit-action', CustomerAddCreditAction);

export default CustomerAddCreditAction;
