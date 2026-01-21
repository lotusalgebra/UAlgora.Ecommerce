import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Currency Symbol Position Action
 * Quick action to toggle currency symbol position.
 */
export class CurrencySymbolPositionAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }

    .position-badge {
      font-size: 11px;
      padding: 2px 6px;
      border-radius: var(--uui-border-radius);
      background: var(--uui-color-surface-emphasis);
      margin-left: 4px;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _position: { type: String, state: true },
    _spaceBetween: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._position = 'Before';
    this._spaceBetween = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (workspace) {
      const currency = workspace.getCurrency();
      this._position = currency?.symbolPosition === 1 ? 'After' : 'Before';
      this._spaceBetween = currency?.spaceBetweenSymbolAndAmount ?? false;
    }
  }

  async _handleToggle() {
    const workspace = document.querySelector('ecommerce-currency-workspace');
    if (!workspace) return;

    const currency = workspace.getCurrency();
    if (!currency?.id) {
      alert('Please save the currency first');
      return;
    }

    // Toggle position
    const newPosition = this._position === 'Before' ? 'After' : 'Before';

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/currency/${currency.id}/update-symbol-position`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ position: newPosition, spaceBetween: this._spaceBetween })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCurrency(result);
        this._position = result.symbolPosition === 1 ? 'After' : 'Before';

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: `Symbol position set to ${newPosition.toLowerCase()} amount`,
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update symbol position');
      }
    } catch (error) {
      console.error('Error updating symbol position:', error);
      alert('Failed to update symbol position');
    } finally {
      this._processing = false;
    }
  }

  render() {
    const example = this._position === 'Before' ? '$100' : '100$';

    return html`
      <uui-button
        look="secondary"
        color="default"
        ?disabled=${this._processing}
        @click=${this._handleToggle}
      >
        <uui-icon name="icon-axis-rotation"></uui-icon>
        Symbol <span class="position-badge">${example}</span>
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-currency-symbol-position-action', CurrencySymbolPositionAction);

export default CurrencySymbolPositionAction;
