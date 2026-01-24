import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Toggle Action
 * Quick action to toggle category visibility.
 */
export class CategoryToggleAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (workspace) {
      const category = workspace.getCategory();
      this._isVisible = category?.isVisible ?? true;
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
      // Get current category data
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${category.id}`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error('Failed to load category');
      }

      const currentCategory = await response.json();
      currentCategory.isVisible = !currentCategory.isVisible;

      // Update category
      const updateResponse = await fetch(`/umbraco/management/api/v1/ecommerce/category/${category.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(currentCategory)
      });

      if (updateResponse.ok) {
        const result = await updateResponse.json();
        workspace.setCategory(result);
        this._isVisible = result.isVisible;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isVisible ? 'Category is now visible' : 'Category is now hidden',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await updateResponse.json();
        alert(error.message || 'Failed to update category');
      }
    } catch (error) {
      console.error('Error toggling category:', error);
      alert('Failed to toggle category visibility');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isVisible ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isVisible ? 'icon-hide' : 'icon-eye'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isVisible ? 'Hide' : 'Show'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-category-toggle-action', CategoryToggleAction);

export default CategoryToggleAction;
