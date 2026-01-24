import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class WebhookCollection extends UmbElementMixin(LitElement) {
  static styles = css`
    :host { display: block; padding: 20px; }
    .collection-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .collection-table { width: 100%; border-collapse: collapse; background: #fff; border: 1px solid #e0e0e0; border-radius: 8px; }
    .collection-table th, .collection-table td { padding: 12px 16px; text-align: left; border-bottom: 1px solid #e0e0e0; }
    .collection-table th { background: #f5f5f5; font-weight: 600; }
    .collection-table tr:hover { background: #f9f9f9; cursor: pointer; }
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .status-active { background: #d4edda; color: #155724; }
    .status-inactive { background: #e2e3e5; color: #383d41; }
    .url-cell { font-family: monospace; font-size: 12px; max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .events-list { display: flex; flex-wrap: wrap; gap: 4px; }
    .event-tag { background: #e9ecef; padding: 2px 6px; border-radius: 3px; font-size: 11px; }
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
    .form-group input, .form-group textarea { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; }
    .checkbox-group { display: grid; grid-template-columns: repeat(2, 1fr); gap: 8px; max-height: 200px; overflow-y: auto; padding: 10px; border: 1px solid #ddd; border-radius: 4px; }
    .checkbox-group label { display: flex; align-items: center; gap: 6px; font-size: 13px; }
  `;

  static properties = {
    _webhooks: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _showModal: { type: Boolean, state: true },
    _editingWebhook: { type: Object, state: true },
    _saving: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._webhooks = [];
    this._loading = true;
    this._showModal = false;
    this._editingWebhook = null;
    this._saving = false;
    this._availableEvents = [
      'order.created', 'order.updated', 'order.paid', 'order.shipped', 'order.delivered', 'order.cancelled',
      'product.created', 'product.updated', 'product.deleted', 'product.stock_low',
      'customer.created', 'customer.updated',
      'return.requested', 'return.approved', 'return.refunded',
      'giftcard.issued', 'giftcard.redeemed'
    ];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadWebhooks();
  }

  async _loadWebhooks() {
    try {
      this._loading = true;
      const response = await fetch('/umbraco/management/api/v1/ecommerce/webhooks', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._webhooks = await response.json();
      }
    } catch (error) {
      console.error('Error loading webhooks:', error);
    } finally {
      this._loading = false;
    }
  }

  _openCreateModal() {
    this._editingWebhook = { name: '', url: '', events: [], isActive: true, secret: '' };
    this._showModal = true;
  }

  _openEditModal(webhook) {
    this._editingWebhook = { ...webhook, events: webhook.events ? JSON.parse(webhook.events) : [] };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingWebhook = null;
  }

  _handleInputChange(field, value) {
    this._editingWebhook = { ...this._editingWebhook, [field]: value };
  }

  _handleEventToggle(event, checked) {
    const events = [...(this._editingWebhook.events || [])];
    if (checked && !events.includes(event)) {
      events.push(event);
    } else if (!checked) {
      const idx = events.indexOf(event);
      if (idx > -1) events.splice(idx, 1);
    }
    this._editingWebhook = { ...this._editingWebhook, events };
  }

  async _saveWebhook() {
    if (!this._editingWebhook.name || !this._editingWebhook.url) {
      alert('Name and URL are required');
      return;
    }
    this._saving = true;
    try {
      const isNew = !this._editingWebhook.id;
      const payload = { ...this._editingWebhook, events: JSON.stringify(this._editingWebhook.events) };
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/webhooks' : `/umbraco/management/api/v1/ecommerce/webhooks/${this._editingWebhook.id}`;
      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      if (!response.ok) throw new Error('Failed to save webhook');
      this._closeModal();
      this._loadWebhooks();
    } catch (error) {
      alert('Failed to save webhook: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _testWebhook(webhook) {
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/webhooks/${webhook.id}/test`, {
        method: 'POST',
        credentials: 'include'
      });
      if (response.ok) {
        alert('Test webhook sent successfully!');
      } else {
        throw new Error('Failed to send test');
      }
    } catch (error) {
      alert('Failed to test webhook: ' + error.message);
    }
  }

  _parseEvents(eventsJson) {
    try {
      return JSON.parse(eventsJson || '[]');
    } catch {
      return [];
    }
  }

  render() {
    return html`
      <div class="collection-header">
        <h2>Webhooks</h2>
        <uui-button look="primary" @click=${this._openCreateModal}><uui-icon name="icon-add"></uui-icon> Add Webhook</uui-button>
      </div>
      ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
        this._webhooks.length === 0 ? html`<div class="empty-state"><uui-icon name="icon-link" style="font-size:48px;"></uui-icon><h3>No webhooks</h3><p>Create webhooks to integrate with external systems</p></div>` :
        html`<table class="collection-table">
          <thead><tr><th>Name</th><th>URL</th><th>Events</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>${this._webhooks.map(w => html`
            <tr @click=${() => this._openEditModal(w)}>
              <td><strong>${w.name}</strong></td>
              <td class="url-cell" title="${w.url}">${w.url}</td>
              <td>
                <div class="events-list">
                  ${this._parseEvents(w.events).slice(0, 3).map(e => html`<span class="event-tag">${e}</span>`)}
                  ${this._parseEvents(w.events).length > 3 ? html`<span class="event-tag">+${this._parseEvents(w.events).length - 3}</span>` : ''}
                </div>
              </td>
              <td><span class="status-badge ${w.isActive ? 'status-active' : 'status-inactive'}">${w.isActive ? 'Active' : 'Inactive'}</span></td>
              <td>
                <uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._testWebhook(w); }}>Test</uui-button>
                <uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._openEditModal(w); }}>Edit</uui-button>
              </td>
            </tr>
          `)}</tbody>
        </table>`}
      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderModal() {
    const isNew = !this._editingWebhook?.id;
    const selectedEvents = this._editingWebhook?.events || [];
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header"><h2>${isNew ? 'Add Webhook' : 'Edit Webhook'}</h2><uui-button look="secondary" compact @click=${this._closeModal}>&times;</uui-button></div>
          <div class="modal-body">
            <div class="form-group"><label>Name *</label><input type="text" .value=${this._editingWebhook?.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} /></div>
            <div class="form-group"><label>URL *</label><input type="url" .value=${this._editingWebhook?.url || ''} @input=${(e) => this._handleInputChange('url', e.target.value)} placeholder="https://your-server.com/webhook" /></div>
            <div class="form-group"><label>Secret (for HMAC signing)</label><input type="text" .value=${this._editingWebhook?.secret || ''} @input=${(e) => this._handleInputChange('secret', e.target.value)} placeholder="Optional secret key" /></div>
            <div class="form-group">
              <label>Events to Subscribe</label>
              <div class="checkbox-group">
                ${this._availableEvents.map(event => html`
                  <label>
                    <input type="checkbox" ?checked=${selectedEvents.includes(event)} @change=${(e) => this._handleEventToggle(event, e.target.checked)} />
                    ${event}
                  </label>
                `)}
              </div>
            </div>
            <div class="form-group">
              <label><input type="checkbox" ?checked=${this._editingWebhook?.isActive} @change=${(e) => this._handleInputChange('isActive', e.target.checked)} /> Active</label>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" @click=${this._saveWebhook} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-webhook-collection', WebhookCollection);
export default WebhookCollection;
