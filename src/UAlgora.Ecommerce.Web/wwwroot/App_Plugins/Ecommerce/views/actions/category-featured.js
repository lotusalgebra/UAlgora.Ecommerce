import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Featured Action
 * Quick action to toggle the featured status of a category.
 */
export class CategoryFeaturedAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isFeatured: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isFeatured = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (workspace) {
      const category = workspace.getCategory();
      this._isFeatured = category?.isFeatured ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      alert('Please save the category first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${category.id}/toggle-featured`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCategory(result);
        this._isFeatured = result.isFeatured;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isFeatured ? 'Category is now featured' : 'Category removed from featured',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle featured status');
      }
    } catch (error) {
      console.error('Error toggling featured status:', error);
      alert('Failed to toggle featured status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isFeatured ? 'warning' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isFeatured ? 'icon-star' : 'icon-favorite'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isFeatured ? 'Featured' : 'Feature'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-category-featured-action', CategoryFeaturedAction);

export default CategoryFeaturedAction;
