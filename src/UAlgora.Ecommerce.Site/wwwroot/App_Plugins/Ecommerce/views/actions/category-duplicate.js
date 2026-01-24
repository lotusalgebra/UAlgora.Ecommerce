import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Duplicate Action
 * Quick action to duplicate a category.
 */
export class CategoryDuplicateAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      alert('Please save the category first');
      return;
    }

    if (!confirm(`Duplicate category "${category.name}"?`)) {
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${category.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Category duplicated as "${result.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the new category
        window.history.pushState({}, '', `/umbraco/section/ecommerce/workspace/category/edit/${result.id}`);
        window.location.reload();
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate category');
      }
    } catch (error) {
      console.error('Error duplicating category:', error);
      alert('Failed to duplicate category');
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
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-category-duplicate-action', CategoryDuplicateAction);

export default CategoryDuplicateAction;
