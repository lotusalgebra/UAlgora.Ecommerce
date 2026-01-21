import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Save Action
 */
export class TaxCategorySaveAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: inline-block;
    }
  `;

  static properties = {
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._saving = false;
  }

  async _handleSave() {
    const workspace = this.closest('ecommerce-tax-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    const isNew = workspace.isNewCategory();

    if (!category.name) {
      this._showNotification('warning', 'Please enter a category name');
      return;
    }

    if (!category.code) {
      this._showNotification('warning', 'Please enter a category code');
      return;
    }

    try {
      this._saving = true;

      const url = isNew
        ? '/umbraco/management/api/v1/ecommerce/tax/category'
        : `/umbraco/management/api/v1/ecommerce/tax/category/${category.id}`;

      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(category)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to save tax category');
      }

      const savedCategory = await response.json();
      workspace.setCategory(savedCategory);

      this._showNotification('positive', isNew ? 'Tax category created' : 'Tax category saved');

      if (isNew && savedCategory.id) {
        window.history.replaceState(null, '', `/umbraco/section/ecommerce/workspace/tax-category/edit/${savedCategory.id}`);
      }
    } catch (error) {
      console.error('Error saving tax category:', error);
      this._showNotification('danger', error.message || 'Failed to save tax category');
    } finally {
      this._saving = false;
    }
  }

  _showNotification(color, message) {
    const event = new CustomEvent('umb-notification', {
      bubbles: true,
      composed: true,
      detail: {
        headline: color === 'positive' ? 'Success' : color === 'warning' ? 'Warning' : 'Error',
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
        ${this._saving ? html`<uui-loader-circle></uui-loader-circle>` : ''}
        Save
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-tax-category-save-action', TaxCategorySaveAction);

export default TaxCategorySaveAction;
