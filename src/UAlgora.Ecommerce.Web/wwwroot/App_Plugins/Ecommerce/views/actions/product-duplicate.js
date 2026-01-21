import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Duplicate Action
 * Quick action to create a copy of the current product.
 */
export class ProductDuplicateAction extends UmbElementMixin(LitElement) {
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

  async _handleDuplicate() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (!workspace) return;

    const product = workspace.getProduct();
    if (!product?.id) {
      alert('Please save the product first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${product.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/product/${product.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedProduct = await response.json();
        alert(`Product duplicated successfully! New product: "${duplicatedProduct.name}"`);

        // Navigate to the duplicated product
        window.location.href = `/umbraco/section/ecommerce/workspace/product/edit/${duplicatedProduct.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate product');
      }
    } catch (error) {
      console.error('Error duplicating product:', error);
      alert('Failed to duplicate product');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-duplicate-action', ProductDuplicateAction);

export default ProductDuplicateAction;
