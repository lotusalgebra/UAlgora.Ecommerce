import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Order Note Action
 * Quick action to add a note to an order.
 */
export class OrderNoteAction extends UmbElementMixin(LitElement) {
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

  async _handleAddNote() {
    const workspace = document.querySelector('ecommerce-order-workspace');
    if (!workspace) return;

    const order = workspace.getOrder();
    if (!order?.id) {
      alert('Please save the order first');
      return;
    }

    const note = prompt('Enter note:');
    if (!note || note.trim() === '') {
      return;
    }

    const isInternal = confirm('Is this an internal note? (OK = Internal, Cancel = Customer visible)');

    this._processing = true;

    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/order/${order.id}/note`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          note: note.trim(),
          isInternal
        })
      });

      if (response.ok) {
        const result = await response.json();
        workspace.setOrder(result);

        const event = new CustomEvent('umb-notification', {
          bubbles: true,
          composed: true,
          detail: {
            headline: 'Success',
            message: 'Note added to order',
            color: 'positive'
          }
        });
        this.dispatchEvent(event);
      } else {
        const error = await response.json();
        alert(error.message || 'Failed to add note');
      }
    } catch (error) {
      console.error('Error adding note:', error);
      alert('Failed to add note');
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
        @click=${this._handleAddNote}
      >
        <uui-icon name="icon-edit"></uui-icon>
        ${this._processing ? 'Adding...' : 'Add Note'}
      </uui-button>
    `;
  }
}

customElements.define('ecommerce-order-note-action', OrderNoteAction);

export default OrderNoteAction;
