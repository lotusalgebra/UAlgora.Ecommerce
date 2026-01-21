import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Payment Method Fees
 * Editor for payment method fee settings.
 */
export class PaymentMethodFees extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
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

    .form-row {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--uui-size-space-4);
    }

    uui-input, uui-select {
      width: 100%;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-2);
    }

    .fee-preview {
      padding: var(--uui-size-space-4);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-top: var(--uui-size-space-4);
    }

    .fee-preview h4 {
      margin: 0 0 var(--uui-size-space-3) 0;
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      text-transform: uppercase;
    }

    .preview-item {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .preview-item:last-child {
      border-bottom: none;
      font-weight: bold;
    }
  `;

  static properties = {
    _method: { type: Object, state: true },
    _previewAmount: { type: Number, state: true }
  };

  constructor() {
    super();
    this._method = {};
    this._previewAmount = 100;
  }

  connectedCallback() {
    super.connectedCallback();

    const checkWorkspace = () => {
      const workspace = this.closest('ecommerce-payment-method-workspace');
      if (workspace) {
        this._method = workspace.getMethod();
        this.requestUpdate();
      } else {
        setTimeout(checkWorkspace, 100);
      }
    };
    checkWorkspace();
  }

  _updateMethod(field, value) {
    this._method = { ...this._method, [field]: value };
    const workspace = this.closest('ecommerce-payment-method-workspace');
    if (workspace) {
      workspace.setMethod(this._method);
    }
  }

  _calculateFee(amount) {
    const feeType = this._method.feeType || 'None';
    if (feeType === 'None') return 0;

    let fee = 0;
    if (feeType === 'FlatFee') {
      fee = this._method.flatFee || 0;
    } else if (feeType === 'Percentage') {
      fee = amount * (this._method.percentageFee || 0) / 100;
    } else if (feeType === 'FlatPlusPercentage') {
      fee = (this._method.flatFee || 0) + (amount * (this._method.percentageFee || 0) / 100);
    }

    if (this._method.maxFee && fee > this._method.maxFee) {
      fee = this._method.maxFee;
    }

    return Math.round(fee * 100) / 100;
  }

  render() {
    const fee = this._calculateFee(this._previewAmount);

    return html`
      <uui-box headline="Fee Configuration">
        <div class="form-group">
          <label>Fee Type</label>
          <uui-select
            .value=${this._method.feeType || 'None'}
            @change=${(e) => this._updateMethod('feeType', e.target.value)}
          >
            <option value="None">No Fee</option>
            <option value="FlatFee">Flat Fee</option>
            <option value="Percentage">Percentage</option>
            <option value="FlatPlusPercentage">Flat + Percentage</option>
          </uui-select>
        </div>

        ${this._method.feeType !== 'None' ? html`
          <div class="form-row">
            ${this._method.feeType === 'FlatFee' || this._method.feeType === 'FlatPlusPercentage' ? html`
              <div class="form-group">
                <label>Flat Fee ($)</label>
                <uui-input
                  type="number"
                  step="0.01"
                  .value=${this._method.flatFee || ''}
                  @input=${(e) => this._updateMethod('flatFee', parseFloat(e.target.value) || null)}
                  placeholder="0.00"
                ></uui-input>
              </div>
            ` : ''}

            ${this._method.feeType === 'Percentage' || this._method.feeType === 'FlatPlusPercentage' ? html`
              <div class="form-group">
                <label>Percentage (%)</label>
                <uui-input
                  type="number"
                  step="0.01"
                  .value=${this._method.percentageFee || ''}
                  @input=${(e) => this._updateMethod('percentageFee', parseFloat(e.target.value) || null)}
                  placeholder="0.00"
                ></uui-input>
              </div>
            ` : ''}

            <div class="form-group">
              <label>Maximum Fee ($)</label>
              <uui-input
                type="number"
                step="0.01"
                .value=${this._method.maxFee || ''}
                @input=${(e) => this._updateMethod('maxFee', parseFloat(e.target.value) || null)}
                placeholder="No limit"
              ></uui-input>
            </div>
          </div>

          <div class="form-group">
            <div class="checkbox-group">
              <uui-checkbox
                ?checked=${this._method.showFeeAtCheckout}
                @change=${(e) => this._updateMethod('showFeeAtCheckout', e.target.checked)}
              ></uui-checkbox>
              <label>Show fee separately at checkout</label>
            </div>
          </div>

          <div class="fee-preview">
            <h4>Fee Preview</h4>
            <div class="form-group">
              <label>Test Order Amount ($)</label>
              <uui-input
                type="number"
                step="0.01"
                .value=${this._previewAmount}
                @input=${(e) => this._previewAmount = parseFloat(e.target.value) || 0}
              ></uui-input>
            </div>
            <div class="preview-item">
              <span>Order Amount</span>
              <span>$${this._previewAmount.toFixed(2)}</span>
            </div>
            <div class="preview-item">
              <span>Processing Fee</span>
              <span>$${fee.toFixed(2)}</span>
            </div>
            <div class="preview-item">
              <span>Total</span>
              <span>$${(this._previewAmount + fee).toFixed(2)}</span>
            </div>
          </div>
        ` : ''}
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-payment-method-fees', PaymentMethodFees);

export default PaymentMethodFees;
