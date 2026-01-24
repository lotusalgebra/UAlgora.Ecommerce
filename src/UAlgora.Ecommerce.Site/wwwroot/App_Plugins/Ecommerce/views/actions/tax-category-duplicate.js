import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Duplicate Action
 * Quick action to create a copy of the current tax category.
 */
export class TaxCategoryDuplicateAction extends UmbElementMixin(LitElement) {
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
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (!workspace) return;

    const category = workspace.getCategory();
    if (!category?.id) {
      alert('Please save the tax category first before duplicating');
      return;
    }

    const confirmed = confirm(`Are you sure you want to duplicate "${category.name}"?`);
    if (!confirmed) return;

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/tax/category/${category.id}/duplicate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const duplicatedCategory = await response.json();

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Tax category duplicated: "${duplicatedCategory.name}"`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);

        // Navigate to the duplicated category
        window.location.href = `/umbraco/section/ecommerce/workspace/tax-category/edit/${duplicatedCategory.id}`;
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to duplicate tax category');
      }
    } catch (error) {
      console.error('Error duplicating tax category:', error);
      alert('Failed to duplicate tax category');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        ?disabled=${this._processing}
        @click=${this._handleDuplicate}
      >
        <uui-icon name="icon-documents"></uui-icon>
        ${this._processing ? 'Duplicating...' : 'Duplicate'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-tax-category-duplicate-action', TaxCategoryDuplicateAction);

export default TaxCategoryDuplicateAction;
