import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Toggle Action
 * Quick action to activate/deactivate a tax category.
 */
export class TaxCategoryToggleAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isActive: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isActive = true;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (workspace) {
      const category = workspace.getCategory();
      this._isActive = category?.isActive ?? true;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      alert('Please save the tax category first');
      return;
    }

    this._processing = true;

    try {
      // Get current category data
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${category.id}`, {
        headers: { 'Accept': 'application/json' }
      });

      if (!response.ok) {
        throw new Error('Failed to load tax category');
      }

      const currentCategory = await response.json();
      currentCategory.isActive = !currentCategory.isActive;

      // Update category
      const updateResponse = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${category.id}`, {
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
        this._isActive = result.isActive;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: result.isActive ? 'Tax category activated' : 'Tax category deactivated',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await updateResponse.json();
        alert(error.message || 'Failed to update tax category');
      }
    } catch (error) {
      console.error('Error toggling tax category:', error);
      alert('Failed to toggle tax category');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isActive ? 'warning' : 'positive'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isActive ? 'icon-block' : 'icon-check'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isActive ? 'Deactivate' : 'Activate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-tax-category-toggle-action', TaxCategoryToggleAction);

export default TaxCategoryToggleAction;
