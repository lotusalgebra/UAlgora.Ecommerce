import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Delivery Estimate Action
 * Quick action to update delivery time estimates.
 */
export class ShippingMethodDeliveryAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .estimate-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _estimatedDaysMin: { type: Number, state: true },
    _estimatedDaysMax: { type: Number, state: true },
    _deliveryEstimateText: { type: String, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._estimatedDaysMin = null;
    this._estimatedDaysMax = null;
    this._deliveryEstimateText = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (workspace) {
      const method = workspace.getShippingMethod();
      this._estimatedDaysMin = method?.estimatedDaysMin;
      this._estimatedDaysMax = method?.estimatedDaysMax;
      this._deliveryEstimateText = method?.deliveryEstimateText;
    }
  }

  _getEstimateDisplay() {
    if (this._deliveryEstimateText) {
      return this._deliveryEstimateText;
    }
    if (this._estimatedDaysMin != null && this._estimatedDaysMax != null) {
      return `${this._estimatedDaysMin}-${this._estimatedDaysMax} days`;
    }
    if (this._estimatedDaysMin != null) {
      return `${this._estimatedDaysMin}+ days`;
    }
    return 'Not set';
  }

  async _handleUpdate() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (!workspace) return;

    const method = workspace.getShippingMethod();
    if (!method?.id) {
      alert('Please save the shipping method first');
      return;
    }

    const currentMin = this._estimatedDaysMin ?? '';
    const currentMax = this._estimatedDaysMax ?? '';
    const currentText = this._deliveryEstimateText ?? '';

    const minInput = prompt(`Minimum delivery days (current: ${currentMin || 'not set'}):`, String(currentMin));
    if (minInput === null) return;

    const maxInput = prompt(`Maximum delivery days (current: ${currentMax || 'not set'}):`, String(currentMax));
    if (maxInput === null) return;

    const textInput = prompt(`Delivery text (e.g., "2-5 business days", current: ${currentText || 'not set'}):`, currentText);
    if (textInput === null) return;

    const estimatedDaysMin = minInput ? parseInt(minInput, 10) : null;
    const estimatedDaysMax = maxInput ? parseInt(maxInput, 10) : null;

    if (minInput && isNaN(estimatedDaysMin)) {
      alert('Please enter a valid number for minimum days');
      return;
    }
    if (maxInput && isNaN(estimatedDaysMax)) {
      alert('Please enter a valid number for maximum days');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/shipping/method/${method.id}/update-delivery`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          estimatedDaysMin,
          estimatedDaysMax,
          deliveryEstimateText: textInput || null
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setShippingMethod(result);
        this._estimatedDaysMin = result.estimatedDaysMin;
        this._estimatedDaysMax = result.estimatedDaysMax;
        this._deliveryEstimateText = result.deliveryEstimateText;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Delivery estimate updated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update delivery estimate');
      }
    } catch (error) {
      console.error('Error updating delivery estimate:', error);
      alert('Failed to update delivery estimate');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleUpdate}
      >
        <uui-icon name="icon-truck"></uui-icon>
        Delivery <span class="estimate-badge">${this._getEstimateDisplay()}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-method-delivery-action', ShippingMethodDeliveryAction);

export default ShippingMethodDeliveryAction;
