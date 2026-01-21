import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Cart Notes Action
 * Quick action to update notes on a cart.
 */
export class CartNotesAction extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: contents;
    }
  `;

  static properties = {
    _processing: { type: Boolean, state: true },
    _hasNotes: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._processing = false;
    this._hasNotes = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (workspace) {
      const cart = workspace.getCart();
      this._hasNotes = !!cart?.notes;
    }
  }

  async _handleNotes() {
    const workspace = document.querySelector('ecommerce-cart-workspace');
    if (!workspace) return;

    const cart = workspace.getCart();
    if (!cart?.id) {
      alert('Cart not found');
      return;
    }

    const currentNotes = cart.notes || '';
    const notes = prompt('Enter notes for this cart:', currentNotes);
    if (notes === null) return; // User cancelled

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/backoffice/ecommerce/cart/${cart.id}/notes`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ notes: notes.trim() || null })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setCart(result);
        this._hasNotes = !!result.notes;

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: notes.trim() ? 'Notes updated' : 'Notes removed',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to update notes');
      }
    } catch (error) {
      console.error('Error updating notes:', error);
      alert('Failed to update notes');
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
        @click=${this._handleNotes}
      >
        <uui-icon name="icon-notepad"></uui-icon>
        ${this._processing ? 'Saving...' : this._hasNotes ? 'Edit Notes' : 'Add Notes'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-cart-notes-action', CartNotesAction);

export default CartNotesAction;
