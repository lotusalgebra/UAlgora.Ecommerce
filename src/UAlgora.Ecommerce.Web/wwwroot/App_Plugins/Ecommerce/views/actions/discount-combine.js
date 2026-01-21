import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Discount Combine Action
 * Quick action to toggle whether a discount can be combined with other discounts.
 */
export class DiscountCombineAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _canCombine: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._canCombine = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (workspace) {
      const discount = workspace.getDiscount();
      this._canCombine = discount?.canCombine ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-discount-workspace');
    if (!workspace) return;

    const discount = workspace.getDiscount();
    if (!discount?.id) {
      alert('Please save the discount first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/discount/${discount.id}/toggle-combine`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setDiscount(result);
        this._canCombine = result.canCombine;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._canCombine
              ? 'Discount can now be combined with others'
              : 'Discount can no longer be combined',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle combine status');
      }
    } catch (error) {
      console.error('Error toggling combine status:', error);
      alert('Failed to toggle combine status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._canCombine ? 'positive' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._canCombine ? 'icon-link' : 'icon-unlink'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._canCombine ? 'Combinable' : 'Exclusive'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-discount-combine-action', DiscountCombineAction);

export default DiscountCombineAction;
