import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

export class EmailTemplateCollection extends UmbElementMixin(LitElement) {
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
    .category-badge { padding: 4px 8px; border-radius: 4px; font-size: 11px; background: #e9ecef; color: #495057; }
    .empty-state { text-align: center; padding: 60px 20px; color: #666; }
    .loading { display: flex; justify-content: center; padding: 40px; }
    .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 10000; }
    .modal { background: white; border-radius: 8px; width: 90%; max-width: 800px; max-height: 90vh; overflow-y: auto; }
    .modal-header { padding: 20px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center; }
    .modal-header h2 { margin: 0; }
    .modal-body { padding: 20px; }
    .modal-footer { padding: 20px; border-top: 1px solid #e0e0e0; display: flex; justify-content: flex-end; gap: 10px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; margin-bottom: 6px; font-weight: 500; }
    .form-group input, .form-group select, .form-group textarea { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box; font-family: inherit; }
    .form-group textarea { min-height: 200px; font-family: monospace; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .preview-frame { border: 1px solid #ddd; border-radius: 4px; min-height: 300px; background: #fff; }
  `;

  static properties = {
    _templates: { type: Array, state: true },
    _loading: { type: Boolean, state: true },
    _showModal: { type: Boolean, state: true },
    _editingTemplate: { type: Object, state: true },
    _saving: { type: Boolean, state: true },
    _eventTypes: { type: Array, state: true }
  };

  constructor() {
    super();
    this._templates = [];
    this._loading = true;
    this._showModal = false;
    this._editingTemplate = null;
    this._saving = false;
    this._eventTypes = [];
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadTemplates();
    this._loadEventTypes();
  }

  async _loadTemplates() {
    try {
      this._loading = true;
      const response = await fetch('/umbraco/management/api/v1/ecommerce/emailtemplates/defaults', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._templates = await response.json();
      }
    } catch (error) {
      console.error('Error loading templates:', error);
    } finally {
      this._loading = false;
    }
  }

  async _loadEventTypes() {
    try {
      const response = await fetch('/umbraco/management/api/v1/ecommerce/emailtemplates/event-types', {
        credentials: 'include',
        headers: { 'Accept': 'application/json' }
      });
      if (response.ok) {
        this._eventTypes = await response.json();
      }
    } catch (error) {
      console.error('Error loading event types:', error);
    }
  }

  _openCreateModal() {
    this._editingTemplate = {
      code: '', name: '', subject: '', bodyHtml: '', eventType: 100, language: 'en-US', isActive: true
    };
    this._showModal = true;
  }

  _openEditModal(template) {
    this._editingTemplate = { ...template };
    this._showModal = true;
  }

  _closeModal() {
    this._showModal = false;
    this._editingTemplate = null;
  }

  _handleInputChange(field, value) {
    this._editingTemplate = { ...this._editingTemplate, [field]: value };
  }

  async _saveTemplate() {
    if (!this._editingTemplate.code || !this._editingTemplate.name || !this._editingTemplate.subject) {
      alert('Code, Name, and Subject are required');
      return;
    }
    this._saving = true;
    try {
      const isNew = !this._editingTemplate.id;
      const url = isNew ? '/umbraco/management/api/v1/ecommerce/emailtemplates' : `/umbraco/management/api/v1/ecommerce/emailtemplates/${this._editingTemplate.id}`;
      const response = await fetch(url, {
        method: isNew ? 'POST' : 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this._editingTemplate)
      });
      if (!response.ok) throw new Error('Failed to save template');
      this._closeModal();
      this._loadTemplates();
    } catch (error) {
      alert('Failed to save template: ' + error.message);
    } finally {
      this._saving = false;
    }
  }

  async _sendTestEmail(template) {
    const email = prompt('Enter email address for test:');
    if (!email) return;
    try {
      const response = await fetch(`/umbraco/management/api/v1/ecommerce/emailtemplates/${template.id}/send-test`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ toEmail: email })
      });
      if (response.ok) {
        alert('Test email sent successfully!');
      } else {
        throw new Error('Failed to send test email');
      }
    } catch (error) {
      alert('Failed to send test email: ' + error.message);
    }
  }

  _getEventCategory(eventType) {
    const value = eventType;
    if (value >= 100 && value < 200) return 'Order';
    if (value >= 200 && value < 300) return 'Customer';
    if (value >= 300 && value < 400) return 'Cart';
    if (value >= 400 && value < 500) return 'Gift Card';
    if (value >= 500 && value < 600) return 'Return';
    return 'Other';
  }

  render() {
    return html`
      <div class="collection-header">
        <h2>Email Templates</h2>
        <uui-button look="primary" @click=${this._openCreateModal}><uui-icon name="icon-add"></uui-icon> Add Template</uui-button>
      </div>
      ${this._loading ? html`<div class="loading"><uui-loader></uui-loader></div>` :
        this._templates.length === 0 ? html`<div class="empty-state"><uui-icon name="icon-message" style="font-size:48px;"></uui-icon><h3>No email templates</h3><p>Create your first email template</p></div>` :
        html`<table class="collection-table">
          <thead><tr><th>Name</th><th>Subject</th><th>Event</th><th>Language</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>${this._templates.map(t => html`
            <tr @click=${() => this._openEditModal(t)}>
              <td><strong>${t.name}</strong><br><small style="color:#666;">${t.code}</small></td>
              <td>${t.subject}</td>
              <td><span class="category-badge">${this._getEventCategory(t.eventType)}</span></td>
              <td>${t.language}</td>
              <td><span class="status-badge ${t.isActive ? 'status-active' : 'status-inactive'}">${t.isActive ? 'Active' : 'Inactive'}</span></td>
              <td>
                <uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._sendTestEmail(t); }}>Test</uui-button>
                <uui-button look="secondary" compact @click=${(e) => { e.stopPropagation(); this._openEditModal(t); }}>Edit</uui-button>
              </td>
            </tr>
          `)}</tbody>
        </table>`}
      ${this._showModal ? this._renderModal() : ''}
    `;
  }

  _renderModal() {
    const isNew = !this._editingTemplate?.id;
    return html`
      <div class="modal-overlay" @click=${(e) => e.target === e.currentTarget && this._closeModal()}>
        <div class="modal">
          <div class="modal-header"><h2>${isNew ? 'Add Email Template' : 'Edit Email Template'}</h2><uui-button look="secondary" compact @click=${this._closeModal}>&times;</uui-button></div>
          <div class="modal-body">
            <div class="form-row">
              <div class="form-group"><label>Code *</label><input type="text" .value=${this._editingTemplate?.code || ''} @input=${(e) => this._handleInputChange('code', e.target.value)} ?disabled=${!isNew} /></div>
              <div class="form-group"><label>Name *</label><input type="text" .value=${this._editingTemplate?.name || ''} @input=${(e) => this._handleInputChange('name', e.target.value)} /></div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Event Type</label>
                <select .value=${this._editingTemplate?.eventType} @change=${(e) => this._handleInputChange('eventType', parseInt(e.target.value))}>
                  ${this._eventTypes.map(et => html`<option value=${et.value}>${et.category} - ${et.name}</option>`)}
                </select>
              </div>
              <div class="form-group"><label>Language</label><input type="text" .value=${this._editingTemplate?.language || 'en-US'} @input=${(e) => this._handleInputChange('language', e.target.value)} /></div>
            </div>
            <div class="form-group"><label>Subject *</label><input type="text" .value=${this._editingTemplate?.subject || ''} @input=${(e) => this._handleInputChange('subject', e.target.value)} /></div>
            <div class="form-group"><label>Body HTML</label><textarea .value=${this._editingTemplate?.bodyHtml || ''} @input=${(e) => this._handleInputChange('bodyHtml', e.target.value)} placeholder="<html>..."></textarea></div>
            <div class="form-group">
              <label><input type="checkbox" ?checked=${this._editingTemplate?.isActive} @change=${(e) => this._handleInputChange('isActive', e.target.checked)} /> Active</label>
            </div>
          </div>
          <div class="modal-footer">
            <uui-button look="secondary" @click=${this._closeModal} ?disabled=${this._saving}>Cancel</uui-button>
            <uui-button look="primary" @click=${this._saveTemplate} ?disabled=${this._saving}>${this._saving ? 'Saving...' : 'Save'}</uui-button>
          </div>
        </div>
      </div>
    `;
  }
}

customElements.define('ecommerce-emailtemplate-collection', EmailTemplateCollection);
export default EmailTemplateCollection;
