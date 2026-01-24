import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Shipping Method Free Threshold Action
 * Quick action to set free shipping threshold.
 */
export class ShippingMethodFreeThresholdAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .threshold-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }

    .free-enabled {
      background: var(--uui-color-positive-emphasis);
      color: var(--uui-color-positive);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _threshold: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._threshold = null;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (workspace) {
      const method = workspace.getShippingMethod();
      this._threshold = method?.freeShippingThreshold;
    }
  }

  _formatCurrency(value) {
    if (value == null) return 'None';
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  async _handleUpdate() {
    const workspace = document.querySelector('ecommerce-shipping-method-workspace');
    if (!workspace) return;

    const method = workspace.getShippingMethod();
    if (!method?.id) {
      alert('Please save the shipping method first');
      return;
    }

    const currentThreshold = this._threshold ?? '';
    const input = prompt(
      `Set free shipping threshold amount.\n` +
      `Current: ${this._formatCurrency(this._threshold)}\n` +
      `Enter amount (leave empty to disable):`,
      String(currentThreshold)
    );
    if (input === null) return;

    let newThreshold = null;
    if (input.trim()) {
      newThreshold = parseFloat(input.replace(/[^0-9.-]/g, ''));
      if (isNaN(newThreshold) || newThreshold < 0) {
        alert('Please enter a valid positive number');
        return;
      }
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/shipping/method/${method.id}/update-free-threshold`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ threshold: newThreshold })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setShippingMethod(result);
        this._threshold = result.freeShippingThreshold;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: newThreshold
              ? `Free shipping enabled for orders over ${this._formatCurrency(newThreshold)}`
              : 'Free shipping threshold disabled',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update free shipping threshold');
      }
    } catch (error) {
      console.error('Error updating free shipping threshold:', error);
      alert('Failed to update free shipping threshold');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const hasThreshold = this._threshold != null;

    return html`
      <uui-button
        look="secondary"
        color="${hasThreshold ? 'positive' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleUpdate}
      >
        <uui-icon name="icon-gift"></uui-icon>
        Free Over
        <span class="threshold-badge ${hasThreshold ? 'free-enabled' : ''}">${this._formatCurrency(this._threshold)}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-shipping-method-free-threshold-action', ShippingMethodFreeThresholdAction);

export default ShippingMethodFreeThresholdAction;
