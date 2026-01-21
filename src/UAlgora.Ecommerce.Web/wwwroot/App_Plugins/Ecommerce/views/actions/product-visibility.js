import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Visibility Action
 * Quick action to toggle the visibility of a product.
 */
export class ProductVisibilityAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isVisible: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isVisible = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (workspace) {
      const product = workspace.getProduct();
      this._isVisible = product?.isVisible ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (!workspace) return;

    const product = workspace.getProduct();
    if (!product?.id) {
      alert('Please save the product first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/products/${product.id}/toggle-visibility`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setProduct(result);
        this._isVisible = result.isVisible;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isVisible ? 'Product is now visible' : 'Product is now hidden',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle visibility');
      }
    } catch (error) {
      console.error('Error toggling visibility:', error);
      alert('Failed to toggle visibility');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isVisible ? 'positive' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isVisible ? 'icon-eye' : 'icon-eye-slash'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isVisible ? 'Visible' : 'Hidden'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-visibility-action', ProductVisibilityAction);

export default ProductVisibilityAction;
