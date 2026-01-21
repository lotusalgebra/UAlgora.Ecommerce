import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Category Move Action
 * Quick action to move a category to a different parent.
 */
export class CategoryMoveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
    }

    .modal-content {
      background: var(--uui-color-surface);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-2);
      min-width: 400px;
      max-width: 500px;
      box-shadow: var(--uui-shadow-depth-3);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--uui-size-layout-1);
      padding-bottom: var(--uui-size-space-4);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .modal-header h3 {
      margin: 0;
    }

    .modal-body {
      margin-bottom: var(--uui-size-layout-1);
    }

    .modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: var(--uui-size-space-3);
      padding-top: var(--uui-size-space-4);
      border-top: 1px solid var(--uui-color-border);
    }

    .category-list {
      max-height: 300px;
      overflow-y: auto;
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
    }

    .category-item {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3) var(--uui-size-space-4);
      cursor: pointer;
      border-bottom: 1px solid var(--uui-color-border);
    }

    .category-item:last-child {
      border-bottom: none;
    }

    .category-item:hover {
      background: var(--uui-color-surface-emphasis);
    }

    .category-item.selected {
      background: var(--uui-color-selected);
    }

    .category-item.disabled {
      opacity: 0.5;
      pointer-events: none;
    }

    .category-item-icon {
      color: var(--uui-color-text-alt);
    }

    .category-item-info {
      flex: 1;
    }

    .category-item-name {
      font-weight: 500;
    }

    .category-item-path {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
    }

    .root-option {
      background: var(--uui-color-surface-alt);
      font-weight: 500;
    }

    .current-parent {
      margin-bottom: var(--uui-size-space-4);
      padding: var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
    }

    .current-parent-label {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      margin-bottom: var(--uui-size-space-1);
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _showModal: { type: Boolean, state: true },
    _categories: { type: Array, state: true },
    _selectedParentId: { type: String, state: true },
    _category: { type: Object, state: true },
    _loading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._showModal = false;
    this._categories = [];
    this._selectedParentId = null;
    this._category = null;
    this._loading = false;
  }

  async _openModal() {
    const workspace = document.querySelector('ecommerce-category-workspace');
    if (!workspace) return;

    this._category = workspace.getCategory();
    if (!this._category?.id) {
      alert('Please save the category first');
      return;
    }

    this._selectedParentId = this._category.parentId || null;
    this._showModal = true;
    await this._loadCategories();
  }

  _closeModal() {
    this._showModal = false;
  }

  async _loadCategories() {
    this._loading = true;

    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/category', {
        headers: { 'Accept': 'application/json' }
      });

      if (response.ok) {
        const data = await response.json();
        // Filter out the current category and its descendants
        this._categories = data.items.filter(c => c.id !== this._category.id);
      }
    } catch (error) {
      console.error('Error loading categories:', error);
    } finally {
      this._loading = false;
    }
  }

  _handleSelect(categoryId) {
    this._selectedParentId = categoryId;
  }

  async _handleMove() {
    if (this._selectedParentId === this._category.parentId) {
      this._closeModal();
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/category/${this._category.id}/move`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          newParentId: this._selectedParentId
        })
      });

      if (response.ok) {
        this._closeModal();

        // Reload the category
        const reloadResponse = await fetch(`/umbraco/management/api/v1/ecommerce/category/${this._category.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (reloadResponse.ok) {
          const updatedCategory = await reloadResponse.json();
          const workspace = document.querySelector('ecommerce-category-workspace');
          if (workspace) {
            workspace.setCategory(updatedCategory);
          }
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Category moved successfully',
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
      alert('Failed to move category');
    } finally {
      this._processing = false;
    }
  }

  _getCurrentParentName() {
    if (!this._category?.parentId) return 'Root (No parent)';
    const parent = this._categories.find(c => c.id === this._category.parentId);
    return parent?.name || 'Unknown';
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        @click=${this._openModal}
      >
        <uui-icon name="icon-enter"></uui-icon>
        Move
      </uui-button>

      ${this._showModal ? html`
        <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
          <div class="modal-content">
            <div class="modal-header">
              <h3>Move Category</h3>
              <uui-button compact look="secondary" @click=${this._closeModal}>
                <uui-icon name="icon-remove"></uui-icon>
              </uui-button>
            </div>

            <div class="modal-body">
              <div class="current-parent">
                <div class="current-parent-label">Current parent:</div>
                <strong>${this._getCurrentParentName()}</strong>
              </div>

              <p style="margin-bottom: var(--uui-size-space-4);">
                Select a new parent for "${this._category?.name}":
              </p>

              ${this._loading ? html`
                <div style="text-align: center; padding: var(--uui-size-layout-2);">
                  <uui-loader></uui-loader>
                </div>
              ` : html`
                <div class="category-list">
                  <div
                    class="category-item root-option ${this._selectedParentId === null ? 'selected' : ''}"
                    @click=${() => this._handleSelect(null)}
                  >
                    <uui-icon name="icon-home" class="category-item-icon"></uui-icon>
                    <div class="category-item-info">
                      <div class="category-item-name">Root (No parent)</div>
                      <div class="category-item-path">Make this a top-level category</div>
                    </div>
                    ${this._selectedParentId === null ? html`
                      <uui-icon name="icon-check" style="color: var(--uui-color-positive);"></uui-icon>
                    ` : ''}
                  </div>

                  ${this._categories.map(category => html`
                    <div
                      class="category-item ${this._selectedParentId === category.id ? 'selected' : ''}"
                      @click=${() => this._handleSelect(category.id)}
                    >
                      <uui-icon name="icon-folder" class="category-item-icon"></uui-icon>
                      <div class="category-item-info">
                        <div class="category-item-name">${category.name}</div>
                        ${category.path ? html`
                          <div class="category-item-path">${category.path}</div>
                        ` : ''}
                      </div>
                      ${this._selectedParentId === category.id ? html`
                        <uui-icon name="icon-check" style="color: var(--uui-color-positive);"></uui-icon>
                      ` : ''}
                    </div>
                  `)}
                </div>
              `}
            </div>

            <div class="modal-footer">
              <uui-button look="secondary" @click=${this._closeModal}>
                Cancel
              </uui-button>
              <uui-button
                look="primary"
                color="positive"
                ?disabled=${this._processing}
                @click=${this._handleMove}
              >
                ${this._processing ? 'Moving...' : 'Move Category'}
              </uui-button>
            </div>
          </div>
        </div>
      ` : ''}
    `;
  }
}

customElements.define('ecommerce-category-move-action', CategoryMoveAction);

export default CategoryMoveAction;
