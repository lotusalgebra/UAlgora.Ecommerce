import { LitElement, html, css } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";

/**
 * Product Media View
 * Manages product images and gallery.
 */
export class ProductMedia extends UmbElementMixin(LitElement) {
  static styles = css`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }

    .media-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      gap: var(--uui-size-layout-1);
      margin-bottom: var(--uui-size-layout-1);
    }

    .media-item {
      position: relative;
      aspect-ratio: 1;
      border: 2px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      overflow: hidden;
      background: var(--uui-color-surface-alt);
      cursor: pointer;
      transition: border-color 0.2s;
    }

    .media-item:hover {
      border-color: var(--uui-color-interactive);
    }

    .media-item.primary {
      border-color: var(--uui-color-positive);
    }

    .media-item img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .media-item-overlay {
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      padding: var(--uui-size-space-2);
      background: linear-gradient(transparent, rgba(0,0,0,0.7));
      display: flex;
      justify-content: space-between;
      align-items: center;
      opacity: 0;
      transition: opacity 0.2s;
    }

    .media-item:hover .media-item-overlay {
      opacity: 1;
    }

    .media-item-actions {
      display: flex;
      gap: var(--uui-size-space-1);
    }

    .primary-badge {
      position: absolute;
      top: var(--uui-size-space-2);
      left: var(--uui-size-space-2);
      background: var(--uui-color-positive);
      color: white;
      padding: var(--uui-size-space-1) var(--uui-size-space-2);
      border-radius: var(--uui-border-radius);
      font-size: var(--uui-type-small-size);
      font-weight: bold;
    }

    .upload-zone {
      border: 2px dashed var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      padding: var(--uui-size-layout-3);
      text-align: center;
      cursor: pointer;
      transition: all 0.2s;
    }

    .upload-zone:hover {
      border-color: var(--uui-color-interactive);
      background: var(--uui-color-surface-emphasis);
    }

    .upload-zone.dragover {
      border-color: var(--uui-color-positive);
      background: var(--uui-color-positive-emphasis);
    }

    .upload-zone uui-icon {
      font-size: 48px;
      margin-bottom: var(--uui-size-space-4);
      color: var(--uui-color-text-alt);
    }

    .upload-zone p {
      margin: 0;
      color: var(--uui-color-text-alt);
    }

    .upload-zone input[type="file"] {
      display: none;
    }

    .empty-state {
      text-align: center;
      padding: var(--uui-size-layout-3);
      color: var(--uui-color-text-alt);
    }

    .empty-state uui-icon {
      font-size: 64px;
      margin-bottom: var(--uui-size-space-4);
    }

    .image-url-input {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-top: var(--uui-size-layout-1);
    }

    .image-url-input uui-input {
      flex: 1;
    }

    .section-title {
      font-size: var(--uui-type-h4-size);
      font-weight: bold;
      margin: var(--uui-size-layout-2) 0 var(--uui-size-layout-1) 0;
      padding-bottom: var(--uui-size-space-2);
      border-bottom: 1px solid var(--uui-color-border);
    }

    .upload-progress {
      margin-top: var(--uui-size-layout-1);
    }

    .upload-progress-item {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-2);
    }

    .upload-progress-bar {
      flex: 1;
      height: 8px;
      background: var(--uui-color-border);
      border-radius: 4px;
      overflow: hidden;
    }

    .upload-progress-bar-fill {
      height: 100%;
      background: var(--uui-color-positive);
      transition: width 0.3s ease;
    }

    .upload-progress-status {
      font-size: var(--uui-type-small-size);
      color: var(--uui-color-text-alt);
      min-width: 60px;
      text-align: right;
    }

    .bulk-actions {
      display: flex;
      gap: var(--uui-size-space-2);
      margin-bottom: var(--uui-size-layout-1);
      padding: var(--uui-size-space-3);
      background: var(--uui-color-surface-alt);
      border-radius: var(--uui-border-radius);
    }

    .bulk-actions span {
      flex: 1;
      font-weight: 500;
    }

    .media-item-checkbox {
      position: absolute;
      top: var(--uui-size-space-2);
      right: var(--uui-size-space-2);
      z-index: 1;
    }

    .media-item.selected {
      border-color: var(--uui-color-interactive);
      box-shadow: 0 0 0 2px var(--uui-color-interactive);
    }

    .media-type-badge {
      position: absolute;
      top: var(--uui-size-space-2);
      left: var(--uui-size-space-2);
      background: rgba(0,0,0,0.7);
      color: white;
      padding: 2px 6px;
      border-radius: 4px;
      font-size: 10px;
      text-transform: uppercase;
    }

    .video-placeholder {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      background: var(--uui-color-surface-alt);
    }

    .video-placeholder uui-icon {
      font-size: 32px;
      margin-bottom: var(--uui-size-space-2);
    }

    .bulk-url-input {
      margin-top: var(--uui-size-layout-1);
    }

    .bulk-url-input textarea {
      width: 100%;
      min-height: 80px;
      padding: var(--uui-size-space-3);
      border: 1px solid var(--uui-color-border);
      border-radius: var(--uui-border-radius);
      font-family: inherit;
      resize: vertical;
    }
  `;

  static properties = {
    _product: { type: Object, state: true },
    _images: { type: Array, state: true },
    _dragover: { type: Boolean, state: true },
    _urlInput: { type: String, state: true },
    _bulkUrlInput: { type: String, state: true },
    _uploading: { type: Boolean, state: true },
    _uploadProgress: { type: Array, state: true },
    _selectedImages: { type: Set, state: true },
    _selectMode: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._product = null;
    this._images = [];
    this._dragover = false;
    this._urlInput = '';
    this._bulkUrlInput = '';
    this._uploading = false;
    this._uploadProgress = [];
    this._selectedImages = new Set();
    this._selectMode = false;
  }

  connectedCallback() {
    super.connectedCallback();
    this._loadFromWorkspace();
  }

  _loadFromWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      this._product = workspace.getProduct();
      this._images = this._product?.imageUrls || [];
    }
  }

  _handleDragOver(e) {
    e.preventDefault();
    this._dragover = true;
  }

  _handleDragLeave(e) {
    e.preventDefault();
    this._dragover = false;
  }

  async _handleDrop(e) {
    e.preventDefault();
    this._dragover = false;

    const files = Array.from(e.dataTransfer.files).filter(f => f.type.startsWith('image/'));
    if (files.length > 0) {
      await this._uploadFiles(files);
    }
  }

  _handleFileSelect(e) {
    const files = Array.from(e.target.files).filter(f => f.type.startsWith('image/'));
    if (files.length > 0) {
      this._uploadFiles(files);
    }
  }

  async _uploadFiles(files) {
    this._uploading = true;
    this._uploadProgress = files.map(f => ({ name: f.name, progress: 0, status: 'uploading' }));

    try {
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        const formData = new FormData();
        formData.append('file', file);

        try {
          const response = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', {
            method: 'POST',
            body: formData
          });

          if (response.ok) {
            const result = await response.json();
            this._images = [...this._images, { url: result.url, type: file.type.startsWith('video/') ? 'video' : 'image', name: file.name }];
            this._uploadProgress = this._uploadProgress.map((p, idx) =>
              idx === i ? { ...p, progress: 100, status: 'complete' } : p
            );
          } else {
            this._uploadProgress = this._uploadProgress.map((p, idx) =>
              idx === i ? { ...p, status: 'error' } : p
            );
            console.error('Failed to upload file:', file.name);
          }
        } catch (err) {
          this._uploadProgress = this._uploadProgress.map((p, idx) =>
            idx === i ? { ...p, status: 'error' } : p
          );
        }
      }

      this._updateWorkspace();
    } catch (error) {
      console.error('Error uploading files:', error);
    } finally {
      this._uploading = false;
      setTimeout(() => { this._uploadProgress = []; }, 2000);
    }
  }

  _toggleSelectMode() {
    this._selectMode = !this._selectMode;
    if (!this._selectMode) {
      this._selectedImages = new Set();
    }
  }

  _toggleImageSelection(index) {
    const newSelected = new Set(this._selectedImages);
    if (newSelected.has(index)) {
      newSelected.delete(index);
    } else {
      newSelected.add(index);
    }
    this._selectedImages = newSelected;
  }

  _selectAllImages() {
    this._selectedImages = new Set(this._images.map((_, i) => i));
  }

  _deselectAllImages() {
    this._selectedImages = new Set();
  }

  _deleteSelectedImages() {
    if (this._selectedImages.size === 0) return;
    if (!confirm(`Delete ${this._selectedImages.size} selected image(s)?`)) return;

    this._images = this._images.filter((_, i) => !this._selectedImages.has(i));
    this._selectedImages = new Set();
    this._selectMode = false;
    this._updateWorkspace();
  }

  _handleBulkUrlAdd() {
    const urls = this._bulkUrlInput
      .split('\n')
      .map(u => u.trim())
      .filter(u => u && (u.startsWith('http://') || u.startsWith('https://')));

    if (urls.length > 0) {
      const newImages = urls
        .filter(url => !this._images.some(img => (typeof img === 'string' ? img : img.url) === url))
        .map(url => ({
          url,
          type: url.match(/\.(mp4|webm|mov|avi)$/i) ? 'video' : 'image',
          name: url.split('/').pop()
        }));

      this._images = [...this._images, ...newImages];
      this._bulkUrlInput = '';
      this._updateWorkspace();
    }
  }

  _getImageUrl(img) {
    return typeof img === 'string' ? img : img.url;
  }

  _getImageType(img) {
    return typeof img === 'string' ? 'image' : (img.type || 'image');
  }

  _handleAddUrl() {
    const url = this._urlInput.trim();
    if (url && !this._images.includes(url)) {
      this._images = [...this._images, url];
      this._urlInput = '';
      this._updateWorkspace();
    }
  }

  _handleSetPrimary(index) {
    if (index === 0) return;

    const newImages = [...this._images];
    const [removed] = newImages.splice(index, 1);
    newImages.unshift(removed);
    this._images = newImages;
    this._updateWorkspace();
  }

  _handleMoveUp(index) {
    if (index === 0) return;

    const newImages = [...this._images];
    [newImages[index - 1], newImages[index]] = [newImages[index], newImages[index - 1]];
    this._images = newImages;
    this._updateWorkspace();
  }

  _handleMoveDown(index) {
    if (index === this._images.length - 1) return;

    const newImages = [...this._images];
    [newImages[index], newImages[index + 1]] = [newImages[index + 1], newImages[index]];
    this._images = newImages;
    this._updateWorkspace();
  }

  _handleDelete(index) {
    this._images = this._images.filter((_, i) => i !== index);
    this._updateWorkspace();
  }

  _updateWorkspace() {
    const workspace = this.closest('ecommerce-product-workspace');
    if (workspace) {
      workspace.updateProduct({ imageUrls: this._images });
    }
  }

  render() {
    return html`
      <uui-box>
        <div slot="headline">Product Images & Videos (${this._images.length})</div>

        ${this._images.length > 0 ? html`
          <div class="bulk-actions">
            <span>${this._selectMode ? `${this._selectedImages.size} selected` : 'Manage media'}</span>
            <uui-button compact look="secondary" @click=${this._toggleSelectMode}>
              ${this._selectMode ? 'Cancel' : 'Select'}
            </uui-button>
            ${this._selectMode ? html`
              <uui-button compact look="secondary" @click=${this._selectAllImages}>Select All</uui-button>
              <uui-button compact look="secondary" @click=${this._deselectAllImages}>Deselect All</uui-button>
              <uui-button compact look="primary" color="danger" @click=${this._deleteSelectedImages}
                ?disabled=${this._selectedImages.size === 0}>
                Delete Selected
              </uui-button>
            ` : ''}
          </div>

          <div class="media-grid">
            ${this._images.map((img, index) => html`
              <div class="media-item ${index === 0 ? 'primary' : ''} ${this._selectedImages.has(index) ? 'selected' : ''}"
                @click=${() => this._selectMode && this._toggleImageSelection(index)}>
                ${this._selectMode ? html`
                  <input type="checkbox" class="media-item-checkbox"
                    .checked=${this._selectedImages.has(index)}
                    @click=${(e) => { e.stopPropagation(); this._toggleImageSelection(index); }} />
                ` : ''}
                ${this._getImageType(img) === 'video' ? html`
                  <div class="video-placeholder">
                    <uui-icon name="icon-video"></uui-icon>
                    <span style="font-size: 10px;">Video</span>
                  </div>
                  <span class="media-type-badge">Video</span>
                ` : html`
                  <img src="${this._getImageUrl(img)}" alt="Product image ${index + 1}" />
                `}
                ${index === 0 ? html`<span class="primary-badge">Primary</span>` : ''}
                ${!this._selectMode ? html`
                  <div class="media-item-overlay">
                    <div class="media-item-actions">
                      ${index !== 0 ? html`
                        <uui-button compact look="secondary" title="Set as primary"
                          @click=${() => this._handleSetPrimary(index)}>
                          <uui-icon name="icon-star"></uui-icon>
                        </uui-button>
                      ` : ''}
                      <uui-button compact look="secondary" ?disabled=${index === 0}
                        title="Move up" @click=${() => this._handleMoveUp(index)}>
                        <uui-icon name="icon-arrow-up"></uui-icon>
                      </uui-button>
                      <uui-button compact look="secondary" ?disabled=${index === this._images.length - 1}
                        title="Move down" @click=${() => this._handleMoveDown(index)}>
                        <uui-icon name="icon-arrow-down"></uui-icon>
                      </uui-button>
                      <uui-button compact look="secondary" color="danger"
                        title="Remove" @click=${() => this._handleDelete(index)}>
                        <uui-icon name="icon-delete"></uui-icon>
                      </uui-button>
                    </div>
                  </div>
                ` : ''}
              </div>
            `)}
          </div>
        ` : html`
          <div class="empty-state">
            <uui-icon name="icon-picture"></uui-icon>
            <p>No images or videos uploaded yet</p>
            <p style="font-size: var(--uui-type-small-size);">Drag & drop files or use the upload zone below</p>
          </div>
        `}

        <div
          class="upload-zone ${this._dragover ? 'dragover' : ''}"
          @dragover=${this._handleDragOver}
          @dragleave=${this._handleDragLeave}
          @drop=${this._handleDrop}
          @click=${() => this.shadowRoot.getElementById('fileInput').click()}
        >
          <uui-icon name="icon-cloud-upload"></uui-icon>
          <p>${this._uploading ? 'Uploading...' : 'Drag & drop images/videos here or click to upload multiple files'}</p>
          <p style="font-size: var(--uui-type-small-size); margin-top: 8px;">Supports: JPG, PNG, GIF, WebP, MP4, WebM</p>
          <input
            id="fileInput"
            type="file"
            accept="image/*,video/*"
            multiple
            @change=${this._handleFileSelect}
          />
        </div>

        ${this._uploadProgress.length > 0 ? html`
          <div class="upload-progress">
            ${this._uploadProgress.map(p => html`
              <div class="upload-progress-item">
                <span style="min-width: 150px; overflow: hidden; text-overflow: ellipsis;">${p.name}</span>
                <div class="upload-progress-bar">
                  <div class="upload-progress-bar-fill" style="width: ${p.progress}%"></div>
                </div>
                <span class="upload-progress-status">
                  ${p.status === 'complete' ? 'Done' : p.status === 'error' ? 'Failed' : `${p.progress}%`}
                </span>
              </div>
            `)}
          </div>
        ` : ''}
      </uui-box>

      <h3 class="section-title">Add by URL</h3>
      <uui-box>
        <p style="color: var(--uui-color-text-alt); margin-bottom: var(--uui-size-layout-1);">
          Add a single image or video by URL:
        </p>
        <div class="image-url-input">
          <uui-input
            placeholder="https://example.com/image.jpg"
            .value=${this._urlInput}
            @input=${(e) => this._urlInput = e.target.value}
            @keypress=${(e) => e.key === 'Enter' && this._handleAddUrl()}
          ></uui-input>
          <uui-button look="primary" @click=${this._handleAddUrl} ?disabled=${!this._urlInput.trim()}>
            Add
          </uui-button>
        </div>

        <div class="bulk-url-input">
          <p style="color: var(--uui-color-text-alt); margin: var(--uui-size-layout-1) 0 var(--uui-size-space-2) 0;">
            Or add multiple URLs (one per line):
          </p>
          <textarea
            placeholder="https://example.com/image1.jpg&#10;https://example.com/image2.jpg&#10;https://example.com/video.mp4"
            .value=${this._bulkUrlInput}
            @input=${(e) => this._bulkUrlInput = e.target.value}
          ></textarea>
          <div style="margin-top: var(--uui-size-space-2);">
            <uui-button look="primary" @click=${this._handleBulkUrlAdd}
              ?disabled=${!this._bulkUrlInput.trim()}>
              Add All URLs
            </uui-button>
          </div>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-product-media', ProductMedia);

export default ProductMedia;
