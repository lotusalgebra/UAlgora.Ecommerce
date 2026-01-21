import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Sort Order Action
 * Quick action to change currency sort order.
 */
export class CurrencySortAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .sort-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _sortOrder: { type: Number, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._sortOrder = 0;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (workspace) {
      const currency = workspace.getCurrency();
      this._sortOrder = currency?.sortOrder ?? 0;
    }
  }

  async _handleSort() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    const input = prompt(`Current sort order: ${this._sortOrder}\nEnter new sort order (lower numbers appear first):`, String(this._sortOrder));
    if (input === null) return;

    const newSortOrder = parseInt(input, 10);
    if (isNaN(newSortOrder)) {
      alert('Please enter a valid number');
      return;
    }

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/update-sort`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ sortOrder: newSortOrder })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCurrency(result);
        this._sortOrder = result.sortOrder;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Sort order updated to ${newSortOrder}`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update sort order');
      }
    } catch (error) {
      console.error('Error updating sort order:', error);
      alert('Failed to update sort order');
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
        @click=${this._handleSort}
      >
        <uui-icon name="icon-navigation-vertical"></uui-icon>
        Sort <span class="sort-badge">${this._sortOrder}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-sort-action', CurrencySortAction);

export default CurrencySortAction;
