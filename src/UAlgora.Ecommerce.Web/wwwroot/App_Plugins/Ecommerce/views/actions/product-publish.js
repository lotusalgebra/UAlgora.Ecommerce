import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Publish Action
 * Quick action to publish or unpublish a product.
 */
export class ProductPublishAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isPublished: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isPublished = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-product-workspace');
    if (workspace) {
      const product = workspace.getProduct();
      this._isPublished = product?.status?.toLowerCase() === 'published';
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

    const action = this._isPublished ? 'unpublish' : 'publish';
    const confirmMessage = this._isPublished
      ? `Unpublish "${product.name}"? It will no longer be visible to customers.`
      : `Publish "${product.name}"? It will become visible to customers.`;

    if (!confirm(confirmMessage)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/products/${product.id}/${action}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setProduct(result);
        this._isPublished = result.status?.toLowerCase() === 'published';

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isPublished ? 'Product published' : 'Product unpublished',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || `Failed to ${action} product`);
      }
    } catch (error) {
      console.error(`Error ${action}ing product:`, error);
      alert(`Failed to ${action} product`);
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="${this._isPublished ? 'secondary' : 'primary'}"
        color="${this._isPublished ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isPublished ? 'icon-eye-slash' : 'icon-globe'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isPublished ? 'Unpublish' : 'Publish'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-product-publish-action', ProductPublishAction);

export default ProductPublishAction;
