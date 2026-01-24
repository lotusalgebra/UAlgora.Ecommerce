import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Tax Category Exempt Action
 * Quick action to toggle the tax exempt status of a category.
 */
export class TaxCategoryExemptAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _isTaxExempt: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._isTaxExempt = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (workspace) {
      const category = workspace.getTaxCategory();
      this._isTaxExempt = category?.isTaxExempt ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-tax-category-workspace');
    if (!workspace) return;

    const category = workspace.getTaxCategory();
    if (!category?.id) {
      alert('Please save the tax category first');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/tax/category/${category.id}/toggle-exempt`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setTaxCategory(result);
        this._isTaxExempt = result.isTaxExempt;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: this._isTaxExempt
              ? 'Category is now tax exempt'
              : 'Category is now taxable',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to toggle exempt status');
      }
    } catch (error) {
      console.error('Error toggling exempt status:', error);
      alert('Failed to toggle exempt status');
    } finally {
      this._processing = false;
    }
  }

  render() {
    return html`
      <uui-button
        look="secondary"
        color="${this._isTaxExempt ? 'positive' : 'default'}"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="${this._isTaxExempt ? 'icon-check' : 'icon-coins'}"></uui-icon>
        ${this._processing ? 'Processing...' : this._isTaxExempt ? 'Tax Exempt' : 'Taxable'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-tax-category-exempt-action', TaxCategoryExemptAction);

export default TaxCategoryExemptAction;
