import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Set Default Action
 * Quick action to set a tax category as the default.
 */
export class TaxCategoryDefaultAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isDefault: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isDefault = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (workspace) {
      const category = workspace.getCategory();
      this._isDefault = category?.isDefault ?? false;
    }
  }

  async _handleSetDefault() {
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      alert('Please save the tax category first');
      return;
    }

    if (this._isDefault) {
      alert('This category is already the default');
      return;
    }

    const confirmed = confirm(`Set "${category.name}" as the default tax category? This will be used for new products.`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${category.id}/set-default`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        // Reload the category to get updated data
        const reloadResponse = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${category.id}`, {
          headers: { 'Accept': 'application/json' }
        });

        if (reloadResponse.ok) {
          const result = await reloadResponse.json();
          workspace.setCategory(result);
          this._isDefault = result.isDefault;
        }

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Category set as default',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to set default category');
      }
    } catch (error) {
      console.error('Error setting default category:', error);
      alert('Failed to set default category');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isDefault ? 'default' : 'positive'}"
        ?disabled=${this._processing || this._isDefault}
        @click=${this._handleSetDefault}
      >
        <uui-icon name="${this._isDefault ? 'icon-check' : 'icon-favorite'}"></uui-icon>
        ${this._processing ? 'Setting...' : this._isDefault ? 'Default Category' : 'Set as Default'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-tax-category-default-action', TaxCategoryDefaultAction);

export default TaxCategoryDefaultAction;
