import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class StoreCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; padding: 20px; }
    .collection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .collection-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; }
    .collection-table th, .collection-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .collection-table th { background: #f5f5f5; font-weight: 600; }
    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .status-active { background: #d4edda; color: #155724; }
    .status-inactive { background: #f8d7da; color: #721c24; }
    .status-trial { background: #fff3cd; color: #856404; }
    .empty-state { text-align: center; padding: 60px 20px; color: #666; }
    .loading { display: flex; justify-content: center; padding: 40px; }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 10000; }
    .modal { background: white; border-radius: 8px; width: 90%; max-width: 600px; max-height: 90vh; overflow-y: auto; }
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
    _stores: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _showModal: { type: Boolean, state: true },
    _editingStore: { type: Object, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._stores = [];
    this._loading = true;
    this._showModal = false;
    this._editingStore = null;
    this._saving = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadStores();
  }

  async _loadStores() {
    try {
      this._loading = true;
      const response = await fetch('/umbraco/management/api/v1/ecommerce/stores', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._stores = await response.json();
      }
    } catch (error) {
      console.error('Error loading stores:', error);
    } finally {
      this._loading = false;
    }
  }

  _openCreateModal() {
    this._editingStore = { code: '', name: '', domain: '', contactEmail: '', defaultCurrencyCode: 'USD', defaultLanguage: 'en-US', status: 0 };
    this._showModal = true;
  }

  _openEditModal(store) {
    this._editingStore = { ...store };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingStore = null;
  }

  _handleInputChange(field, value) {
    this._editingStore = { ...this._editingStore, [field]: value };
  }

  async _saveStore() {
    if (!this._editingStore.code || !this._editingStore.name) {
      alert('Code and Name are required');
      return;
    }
    this._saving = true;
    try {
      const isNew = !this._editingStore.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/stores' : `/umbraco/management/api/v1/ecommerce/stores/${this._editingStore.id}`;
      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._editingStore)
      });
      if (!response.ok) throw new Error('Failed to save store');
      this._closeModal();
      this._loadStores();
    } catch (error) {
      alert('Failed to save store: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  _getStatusClass(status) {
    return status === 0 ? 'status-active' : status === 2 ? 'status-trial' : 'status-inactive';
  }

  _getStatusText(status) {
    return status === 0 ? 'Active' : status === 2 ? 'Trial' : 'Suspended';
  }

  render() {
    return html`
      <div class="collection-header">
        <h2>Stores</h2>
        <uui-button look="primary" @click=${this._openCreateModal}><uui-icon name="icon-add"></uui-icon> Add Store</uui-button>
      </div>
      ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
        this._stores.length === 0 ? html`<div class="empty-state"><uui-icon name="icon-store" style="font-size:48px;"></uui-icon><h3>No stores</h3><p>Create your first store to get started</p></div>` :
        html`<table class="collection-table">
          <thead><tr><th>Code</th><th>Name</th><th>Domain</th><th>Currency</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>${this._stores.map(s => html`
            <tr @click=${() => this._openEditModal(s)}>
              <td><strong>${s.code}</strong></td>
              <td>${s.name}</td>
              <td>${s.domain || '-'}</td>
              <td>${s.defaultCurrencyCode}</td>
              <td><span class="status-badge ${this._getStatusClass(s.status)}">${this._getStatusText(s.status)}</span></td>
              <td><uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._openEditModal(s); }}>Edit</uui-button></td>
            </tr>
          `)}</tbody>
        </table>`}
      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderModal() {
    const isNew = !this._editingStore?.id;
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header"><h2>${isNew ? 'Add Store' : 'Edit Store'}</h2><uui-button look="secondary" compact @click=${this._closeModal}>&times;</uui-button></div>
          <div class="modal-body">
            <div class="form-row">
              <div class="form-group"><label>Code *</label><input type="text" .value=${this._editingStore?.code || ''} @input=${(e) => this._handleInputChange('code', e.target.value)} ?disabled=${!isNew} /></div>
              <div class="form-group"><label>Name *</label><input type="text" .value=${this._editingStore?.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} /></div>
            </div>
            <div class="form-group"><label>Domain</label><input type="text" .value=${this._editingStore?.domain || ''} @input=${(e) => this._handleInputChange('domain', e.target.value)} placeholder="store.example.com" /></div>
            <div class="form-row">
              <div class="form-group"><label>Contact Email</label><input type="email" .value=${this._editingStore?.contactEmail || ''} @input=${(e) => this._handleInputChange('contactEmail', e.target.value)} /></div>
              <div class="form-group"><label>Support Email</label><input type="email" .value=${this._editingStore?.supportEmail || ''} @input=${(e) => this._handleInputChange('supportEmail', e.target.value)} /></div>
            </div>
            <div class="form-row">
              <div class="form-group"><label>Currency</label><input type="text" .value=${this._editingStore?.defaultCurrencyCode || 'USD'} @input=${(e) => this._handleInputChange('defaultCurrencyCode', e.target.value)} /></div>
              <div class="form-group"><label>Language</label><input type="text" .value=${this._editingStore?.defaultLanguage || 'en-US'} @input=${(e) => this._handleInputChange('defaultLanguage', e.target.value)} /></div>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" @click=${this._saveStore} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-store-collection', StoreCollection);
export default StoreCollection;
