import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class GiftCardCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; padding: 20px; }
    .collection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .collection-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; }
    .collection-table th, .collection-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .collection-table th { background: #f5f5f5; font-weight: 600; }
    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .status-active { background: #d4edda; color: #155724; }
    .status-redeemed { background: #cce5ff; color: #004085; }
    .status-expired { background: #f8d7da; color: #721c24; }
    .status-disabled { background: #e2e3e5; color: #383d41; }
    .code-cell { font-family: monospace; font-weight: bold; }
    .balance { font-weight: 600; color: #28a745; }
    .empty-state { text-align: center; padding: 60px 20px; color: #666; }
    .loading { display: flex; justify-content: center; padding: 40px; }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 10000; }
    .modal { background: white; border-radius: 8px; width: 90%; max-width: 500px; max-height: 90vh; overflow-y: auto; }
    .modal-header { padding: 20px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; }
    .modal-header h2 { margin: 0; }
    .modal-body { padding: 20px; }
    .modal-footer { padding: 20px; border-top: 1px solid #e0e0e0; display: flex; justify-content: flex-end; gap: 10px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; }
    .form-group input, .form-group select { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
  `;

  static properties = {
    _giftCards: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _showModal: { type: Boolean, state: true },
    _editingCard: { type: Object, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._giftCards = [];
    this._loading = true;
    this._showModal = false;
    this._editingCard = null;
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadGiftCards();
  }

  async _loadGiftCards() {
    try {
      this._loading = true;
      const response = await fetch('/umbraco/management/api/v1/ecommerce/giftcards/active', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._giftCards = await response.json();
      }
    } catch (error) {
      console.error('Error loading gift cards:', error);
    } finally {
      this._loading = false;
    }
  }

  _openCreateModal() {
    this._editingCard = { initialValue: 50, currencyCode: 'USD', expiresAt: '', notes: '' };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingCard = null;
  }

  _handleInputChange(field, value) {
    this._editingCard = { ...this._editingCard, [field]: value };
  }

  async _saveGiftCard() {
    if (!this._editingCard.initialValue || this._editingCard.initialValue <= 0) {
      alert('Initial value must be greater than 0');
      return;
    }
    this._saving = true;
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/giftcards/generate', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._editingCard)
      });
      if (!response.ok) throw new Error('Failed to create gift card');
      const newCard = await response.json();
      alert(`Gift card created! Code: ${newCard.code}`);
      this._closeModal();
      this._loadGiftCards();
    } catch (error) {
      alert('Failed to create gift card: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  _formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount || 0);
  }

  _getStatusClass(status) {
    switch(status) {
      case 0: return 'status-active';
      case 1: return 'status-redeemed';
      case 2: return 'status-expired';
      default: return 'status-disabled';
    }
  }

  _getStatusText(status) {
    switch(status) {
      case 0: return 'Active';
      case 1: return 'Redeemed';
      case 2: return 'Expired';
      default: return 'Disabled';
    }
  }

  render() {
    return html`
      <div class="collection-header">
        <h2>Gift Cards</h2>
        <uui-button look="primary" @click=${this._openCreateModal}><uui-icon name="icon-add"></uui-icon> Generate Gift Card</uui-button>
      </div>
      ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
        this._giftCards.length === 0 ? html`<div class="empty-state"><uui-icon name="icon-gift" style="font-size:48px;"></uui-icon><h3>No gift cards</h3><p>Generate your first gift card</p></div>` :
        html`<table class="collection-table">
          <thead><tr><th>Code</th><th>Initial Value</th><th>Balance</th><th>Status</th><th>Expires</th><th>Used</th></tr></thead>
          <tbody>${this._giftCards.map(gc => html`
            <tr>
              <td class="code-cell">${gc.code}</td>
              <td>${this._formatCurrency(gc.initialValue)}</td>
              <td class="balance">${this._formatCurrency(gc.balance)}</td>
              <td><span class="status-badge ${this._getStatusClass(gc.status)}">${this._getStatusText(gc.status)}</span></td>
              <td>${gc.expiresAt ? new Date(gc.expiresAt).toLocaleDateString() : 'Never'}</td>
              <td>${gc.usageCount || 0} times</td>
            </tr>
          `)}</tbody>
        </table>`}
      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderModal() {
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header"><h2>Generate Gift Card</h2><uui-button look="secondary" compact @click=${this._closeModal}>&times;</uui-button></div>
          <div class="modal-body">
            <div class="form-row">
              <div class="form-group"><label>Initial Value *</label><input type="number" min="1" .value=${this._editingCard?.initialValue || 50} @input=${(e) => this._handleInputChange('initialValue', parseFloat(e.target.value))} /></div>
              <div class="form-group"><label>Currency</label><input type="text" .value=${this._editingCard?.currencyCode || 'USD'} @input=${(e) => this._handleInputChange('currencyCode', e.target.value)} /></div>
            </div>
            <div class="form-group"><label>Expires At (optional)</label><input type="date" .value=${this._editingCard?.expiresAt || ''} @input=${(e) => this._handleInputChange('expiresAt', e.target.value)} /></div>
            <div class="form-group"><label>Notes</label><input type="text" .value=${this._editingCard?.notes || ''} @input=${(e) => this._handleInputChange('notes', e.target.value)} /></div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" @click=${this._saveGiftCard} ?disabled=${this._saving}>${this._saving ? 'Generating...' : 'Generate'}</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-giftcard-collection', GiftCardCollection);
export default GiftCardCollection;
