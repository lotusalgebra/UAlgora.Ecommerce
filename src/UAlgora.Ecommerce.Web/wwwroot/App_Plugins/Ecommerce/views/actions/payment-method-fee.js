import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Fee Action
 * Quick action to update fee settings for payment methods.
 */
export class PaymentMethodFeeAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .fee-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .has-fee {
      background: var(--uui-color-warning-emphasis);
      color: var(--uui-color-warning);
    }

    .no-fee {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _feeType: { type: String, state: true },
    _flatFee: { type: Number, state: true },
    _percentageFee: { type: Number, state: true },
    _feeSummary: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._feeType = 'None';
    this._flatFee = 0;
    this._percentageFee = 0;
    this._feeSummary = 'No fee';
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (workspace) {
      const method = workspace.getPaymentMethod();
      this._feeType = method?.feeType ?? 'None';
      this._flatFee = method?.flatFee ?? 0;
      this._percentageFee = method?.percentageFee ?? 0;
      this._updateFeeSummary();
    }
  }

  _updateFeeSummary() {
    switch (this._feeType) {
      case 'FlatFee':
        this._feeSummary = `$${this._flatFee?.toFixed(2) ?? '0.00'}`;
        break;
      case 'Percentage':
        this._feeSummary = `${this._percentageFee ?? 0}%`;
        break;
      case 'FlatPlusPercentage':
        this._feeSummary = `$${this._flatFee?.toFixed(2) ?? '0.00'} + ${this._percentageFee ?? 0}%`;
        break;
      default:
        this._feeSummary = 'No fee';
    }
  }

  async _handleFee() {
    const workspace = document.querySelector('ecommerce-payment-method-workspace');
    if (!workspace) return;

    const method = workspace.getPaymentMethod();
    if (!method?.id) {
      alert('Please save the payment method first');
      return;
    }

    const feeTypeOptions = ['None', 'FlatFee', 'Percentage', 'FlatPlusPercentage'];
    const currentIndex = feeTypeOptions.indexOf(this._feeType);

    const feeTypeInput = prompt(
      `Current fee type: ${this._feeType}\n\nSelect fee type:\n0 - None\n1 - Flat Fee\n2 - Percentage\n3 - Flat + Percentage\n\nEnter number (0-3):`,
      String(currentIndex >= 0 ? currentIndex : 0)
    );

    if (feeTypeInput === null) return;

    const feeTypeIndex = parseInt(feeTypeInput, 10);
    if (isNaN(feeTypeIndex) || feeTypeIndex < 0 || feeTypeIndex > 3) {
      alert('Please enter a valid number (0-3)');
      return;
    }

    const newFeeType = feeTypeOptions[feeTypeIndex];
    let newFlatFee = this._flatFee;
    let newPercentageFee = this._percentageFee;

    if (newFeeType === 'FlatFee' || newFeeType === 'FlatPlusPercentage') {
      const flatInput = prompt('Enter flat fee amount:', String(newFlatFee ?? 0));
      if (flatInput === null) return;
      newFlatFee = parseFloat(flatInput);
      if (isNaN(newFlatFee) || newFlatFee < 0) {
        alert('Please enter a valid fee amount');
        return;
      }
    }

    if (newFeeType === 'Percentage' || newFeeType === 'FlatPlusPercentage') {
      const percentInput = prompt('Enter percentage fee (e.g., 2.9 for 2.9%):', String(newPercentageFee ?? 0));
      if (percentInput === null) return;
      newPercentageFee = parseFloat(percentInput);
      if (isNaN(newPercentageFee) || newPercentageFee < 0 || newPercentageFee > 100) {
        alert('Please enter a valid percentage (0-100)');
        return;
      }
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/payment/method/${method.id}/update-fee`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          feeType: newFeeType,
          flatFee: newFlatFee,
          percentageFee: newPercentageFee,
          maxFee: method.maxFee
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setPaymentMethod(result);
        this._feeType = result.feeType;
        this._flatFee = result.flatFee;
        this._percentageFee = result.percentageFee;
        this._updateFeeSummary();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Fee settings updated`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update fee settings');
      }
    } catch (error) {
      console.error('Error updating fee settings:', error);
      alert('Failed to update fee settings');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const hasFee = this._feeType !== 'None';

    return html`
      <uui-button
        look="secondary"
        color="${hasFee ? 'warning' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleFee}
      >
        <uui-icon name="icon-coins-dollar-alt"></uui-icon>
        Fee
        <span class="fee-badge ${hasFee ? 'has-fee' : 'no-fee'}">
          ${this._feeSummary}
        </span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-payment-method-fee-action', PaymentMethodFeeAction);

export default PaymentMethodFeeAction;
