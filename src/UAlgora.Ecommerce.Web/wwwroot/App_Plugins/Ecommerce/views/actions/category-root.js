import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Move to Root Action
 * Quick action to move a category to the root level.
 */
export class CategoryRootAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isRoot: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isRoot = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (workspace) {
      const category = workspace.getCategory();
      this._isRoot = category?.isRoot ?? !category?.parentId;
    }
  }

  async _handleMoveToRoot() {
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      alert('Please save the category first');
      return;
    }

    if (this._isRoot) {
      alert('Category is already at root level');
      return;
    }

    if (!confirm(`Move "${category.name}" to root level?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${category.id}/move-to-root`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCategory(result);
        this._isRoot = true;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Category moved to root level',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to move category');
      }
    } catch (error) {
      console.error('Error moving category:', error);
      alert('Failed to move category to root');
    } finally {
      this._processing = false;
    }
  }

  render() {
    if (this._isRoot) {
      return html``;
    }

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleMoveToRoot}
      >
        <uui-icon name="icon-home"></uui-icon>
        ${this._processing ? 'Moving...' : 'Move to Root'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-category-root-action', CategoryRootAction);

export default CategoryRootAction;
