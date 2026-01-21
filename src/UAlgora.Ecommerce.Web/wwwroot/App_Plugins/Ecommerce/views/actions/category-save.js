import { LitElement, html } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Save Action
 * Handles saving category data to the backend.
 */
export class CategorySaveAction extends UmbElementMixin(LitElement) {
  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _handleSave() {
    const workspace = this.closest('ecommerce-category-workspace');
    if (!workspace) {
      console.error('Could not find category workspace');
      return;
    }

    const category = workspace.getCategory();
    if (!category) {
      console.error('No category data to save');
      return;
    }

    // Validate required fields
    if (!category.name?.trim()) {
      this._showNotification('error', 'Category name is required');
      return;
    }

    try {
      this._saving = true;

      const isNew = workspace.isNew();
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/category'
        : `/umbraco/management/api/v1/ecommerce/category/${category.id}`;

      const method = isNew ? 'POST' : 'PUT';

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(this._prepareCategoryData(category))
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save category');
      }

      const savedCategory = await response.json();
      workspace.setCategory(savedCategory);

      this._showNotification('positive', isNew ? 'Category created successfully' : 'Category saved successfully');

      // If it was a new category, update the URL
      if (isNew && savedCategory.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/category/edit/${savedCategory.id}`);
      }
    } catch (error) {
      console.error('Error saving category:', error);
      this._showNotification('danger', error.message || 'Failed to save category');
    } finally {
      this._saving = false;
    }
  }

  _prepareCategoryData(category) {
    return {
      name: category.name?.trim(),
      slug: category.slug?.trim() || null,
      description: category.description || null,
      parentId: category.parentId || null,
      isVisible: category.isVisible ?? true,
      isFeatured: category.isFeatured ?? false,
      sortOrder: category.sortOrder || 0,
      imageUrl: category.imageUrl || null,
      metaTitle: category.metaTitle || null,
      metaDescription: category.metaDescription || null
    };
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : 'Error',
        message,
        color
      }
    });
    this.dispatchEvent(event);
  }

  render() {
    return html`
      <uui-button
        look="primary"
        color="positive"
        ?disabled=${this._saving}
        @click=${this._handleSave}
      >
        ${this._saving
          ? html`<uui-loader-circle></uui-loader-circle>`
          : html`<uui-icon name="icon-save"></uui-icon>`
        }
        ${this._saving ? 'Saving...' : 'Save'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-category-save-action', CategorySaveAction);

export default CategorySaveAction;
