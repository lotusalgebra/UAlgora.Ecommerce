import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class CategoryCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: 20px;
    }

    .collection-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      gap: 16px;
    }

    .search-box { flex: 1; max-width: 400px; }
    .search-box uui-input { width: 100%; }

    .collection-table {
      width: 100%;
      border-collapse: collapse;
      background: #fff;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
    }

    .collection-table th, .collection-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid #e0e0e0;
    }

    .collection-table th {
      background: #f5f5f5;
      font-weight: 600;
    }

    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .collection-table tr:last-child td { border-bottom: none; }

    .category-image {
      width: 50px; height: 50px;
      object-fit: cover;
      border-radius: 4px;
      background: #f0f0f0;
    }

    .badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      margin-right: 4px;
    }
    .badge-visible { background: #27ae60; color: white; }
    .badge-hidden { background: #e74c3c; color: white; }
    .badge-featured { background: #f39c12; color: white; }

    .empty-state {
      text-align: center;
      padding: 60px 20px;
      color: #666;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 40px;
    }

    /* Modal Styles */
    .modal-overlay {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }

    .modal {
      background: white;
      border-radius: 8px;
      width: 90%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    }

    .modal-header {
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .modal-header h2 { margin: 0; font-size: 20px; }

    .modal-body { padding: 20px; }

    .modal-footer {
      padding: 20px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      justify-content: flex-end;
      gap: 10px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-group label {
      display: block;
      margin-bottom: 6px;
      font-weight: 500;
      color: #333;
    }

    .form-group input, .form-group textarea, .form-group select {
      width: 100%;
      padding: 10px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
      box-sizing: border-box;
    }

    .form-group textarea { min-height: 80px; resize: vertical; }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .checkbox-group {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .checkbox-group input { width: auto; }
  `;

  static properties = {
    _categories: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _searchTerm: { type: String, state: true },
    _totalCount: { type: Number, state: true },
    _showModal: { type: Boolean, state: true },
    _editingCategory: { type: Object, state: true },
    _saving: { type: Boolean, state: true },
    _parentCategories: { type: Array, state: true }
  };

  constructor() {
    super();
    this._categories = [];
    this._loading = true;
    this._searchTerm = '';
    this._totalCount = 0;
    this._showModal = false;
    this._editingCategory = null;
    this._saving = false;
    this._parentCategories = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadCategories();
  }

  async _loadCategories() {
    try {
      this._loading = true;

      const response = await fetch('/umbraco/management/api/v1/ecommerce/category', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) throw new Error('Failed to load categories');

      const data = await response.json();
      this._categories = data.items || [];
      this._totalCount = data.total || 0;
      // Store parent categories for dropdown
      this._parentCategories = this._categories.filter(c => !c.parentId);
    } catch (error) {
      console.error('Error loading categories:', error);
      this._categories = [];
    } finally {
      this._loading = false;
    }
  }

  _handleSearch(event) {
    this._searchTerm = event.target.value.toLowerCase();
  }

  _getFilteredCategories() {
    if (!this._searchTerm) return this._categories;
    return this._categories.filter(c =>
      c.name.toLowerCase().includes(this._searchTerm) ||
      (c.slug && c.slug.toLowerCase().includes(this._searchTerm))
    );
  }

  _openCreateModal() {
    this._editingCategory = {
      name: '', slug: '', description: '',
      isVisible: true, isFeatured: false,
      sortOrder: 0, parentId: null,
      imageUrl: '', metaTitle: '', metaDescription: ''
    };
    this._showModal = true;
  }

  _openEditModal(category) {
    this._editingCategory = { ...category };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingCategory = null;
  }

  _handleInputChange(field, value) {
    this._editingCategory = { ...this._editingCategory, [field]: value };
  }

  async _saveCategory() {
    if (!this._editingCategory.name) {
      alert('Name is required');
      return;
    }

    this._saving = true;
    try {
      const isNew = !this._editingCategory.id;
      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/category'
        : `/umbraco/management/api/v1/ecommerce/category/${this._editingCategory.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
        body: JSON.stringify(this._editingCategory)
      });

      if (!response.ok) {
        const error = await response.text();
        throw new Error(error || 'Failed to save category');
      }

      this._closeModal();
      this._loadCategories();
    } catch (error) {
      console.error('Error saving category:', error);
      alert('Failed to save category: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _deleteCategory(category) {
    if (!confirm(`Are you sure you want to delete "${category.name}"?`)) return;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${category.id}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (!response.ok) throw new Error('Failed to delete category');
      this._loadCategories();
    } catch (error) {
      console.error('Error deleting category:', error);
      alert('Failed to delete category');
    }
  }

  render() {
    return html`
      <div class="collection-header">
        <div class="search-box">
          <uui-input label="Search categories" placeholder="Search categories..." .value=${this._searchTerm} @input=${this._handleSearch}>
            <uui-icon name="icon-search" slot="prepend"></uui-icon>
          </uui-input>
        </div>
        <uui-button look="primary" label="Add Category" @click=${this._openCreateModal}>
          <uui-icon name="icon-add"></uui-icon> Add Category
        </uui-button>
      </div>

      ${this._loading
        ? html`<div class="loading"><uui-loader></uui-loader></div>`
        : this._getFilteredCategories().length === 0
          ? this._renderEmptyState()
          : this._renderCategoriesTable()
      }

      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderEmptyState() {
    return html`
      <div class="empty-state">
        <uui-icon name="icon-folders" style="font-size: 48px;"></uui-icon>
        <h3>No categories found</h3>
        <p>${this._searchTerm ? 'Try adjusting your search' : 'Get started by adding your first category'}</p>
        ${!this._searchTerm ? html`
          <uui-button look="primary" label="Add Category" @click=${this._openCreateModal}>Add Your First Category</uui-button>
        ` : ''}
      </div>
    `;
  }

  _renderCategoriesTable() {
    const filteredCategories = this._getFilteredCategories();

    return html`
      <table class="collection-table">
        <thead>
          <tr>
            <th style="width: 60px;"></th>
            <th>Name</th>
            <th>Slug</th>
            <th>Status</th>
            <th>Sort Order</th>
            <th style="width: 100px;">Actions</th>
          </tr>
        </thead>
        <tbody>
          ${filteredCategories.map(category => html`
            <tr>
              <td>
                ${category.imageUrl
                  ? html`<img class="category-image" src="${category.imageUrl}" alt="${category.name}" />`
                  : html`<div class="category-image" style="display: flex; align-items: center; justify-content: center;"><uui-icon name="icon-folder"></uui-icon></div>`
                }
              </td>
              <td>
                <strong>${category.name}</strong>
                ${category.parentId ? html`<div style="font-size: 12px; color: #666;">Subcategory</div>` : ''}
              </td>
              <td>${category.slug || '-'}</td>
              <td>
                <span class="badge ${category.isVisible !== false ? 'badge-visible' : 'badge-hidden'}">
                  ${category.isVisible !== false ? 'Visible' : 'Hidden'}
                </span>
                ${category.isFeatured ? html`<span class="badge badge-featured">Featured</span>` : ''}
              </td>
              <td>${category.sortOrder ?? 0}</td>
              <td>
                <uui-button look="secondary" compact label="Edit" @click=${() => this._openEditModal(category)}>Edit</uui-button>
              </td>
            </tr>
          `)}
        </tbody>
      </table>
    `;
  }

  _renderModal() {
    const isNew = !this._editingCategory?.id;
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header">
            <h2>${isNew ? 'Add New Category' : 'Edit Category'}</h2>
            <uui-button look="secondary" compact label="Close" @click=${this._closeModal}>&times;</uui-button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Category Name *</label>
              <input type="text" .value=${this._editingCategory?.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Slug</label>
                <input type="text" .value=${this._editingCategory?.slug || ''} @input=${(e) => this._handleInputChange('slug', e.target.value)} />
              </div>
              <div class="form-group">
                <label>Parent Category</label>
                <select .value=${this._editingCategory?.parentId || ''} @change=${(e) => this._handleInputChange('parentId', e.target.value || null)}>
                  <option value="">None (Top Level)</option>
                  ${this._parentCategories.filter(c => c.id !== this._editingCategory?.id).map(c => html`
                    <option value="${c.id}" ?selected=${this._editingCategory?.parentId === c.id}>${c.name}</option>
                  `)}
                </select>
              </div>
            </div>
            <div class="form-group">
              <label>Description</label>
              <textarea .value=${this._editingCategory?.description || ''} @input=${(e) => this._handleInputChange('description', e.target.value)}></textarea>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Sort Order</label>
                <input type="number" .value=${this._editingCategory?.sortOrder || 0} @input=${(e) => this._handleInputChange('sortOrder', parseInt(e.target.value) || 0)} />
              </div>
              <div class="form-group">
                <label>Image URL</label>
                <input type="text" .value=${this._editingCategory?.imageUrl || ''} @input=${(e) => this._handleInputChange('imageUrl', e.target.value)} />
              </div>
            </div>
            <div class="form-group">
              <label>Meta Title (SEO)</label>
              <input type="text" .value=${this._editingCategory?.metaTitle || ''} @input=${(e) => this._handleInputChange('metaTitle', e.target.value)} />
            </div>
            <div class="form-group">
              <label>Meta Description (SEO)</label>
              <textarea .value=${this._editingCategory?.metaDescription || ''} @input=${(e) => this._handleInputChange('metaDescription', e.target.value)}></textarea>
            </div>
            <div class="form-row">
              <div class="form-group checkbox-group">
                <input type="checkbox" id="isVisible" .checked=${this._editingCategory?.isVisible !== false} @change=${(e) => this._handleInputChange('isVisible', e.target.checked)} />
                <label for="isVisible">Visible</label>
              </div>
              <div class="form-group checkbox-group">
                <input type="checkbox" id="isFeatured" .checked=${this._editingCategory?.isFeatured || false} @change=${(e) => this._handleInputChange('isFeatured', e.target.checked)} />
                <label for="isFeatured">Featured</label>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" label="Cancel" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" label="Save" @click=${this._saveCategory} ?disabled=${this._saving}>
              ${this._saving ? 'Saving...' : 'Save Category'}
            </uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-category-collection', CategoryCollection);
export default CategoryCollection;
