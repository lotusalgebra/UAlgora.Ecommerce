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
  `;

  static properties = {
    _product: { type: Object, state: true },
    _images: { type: Array, state: true },
    _dragover: { type: Boolean, state: true },
    _urlInput: { type: String, state: true },
    _uploading: { type: Boolean, state: true }
  };

  constructor() {
    super();
    this._product = null;
    this._images = [];
    this._dragover = false;
    this._urlInput = '';
    this._uploading = false;
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

    try {
      for (const file of files) {
        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch('/umbraco/management/api/v1/ecommerce/media/upload', {
          method: 'POST',
          body: formData
        });

        if (response.ok) {
          const result = await response.json();
          this._images = [...this._images, result.url];
        } else {
          console.error('Failed to upload file:', file.name);
        }
      }

      this._updateWorkspace();
    } catch (error) {
      console.error('Error uploading files:', error);
    } finally {
      this._uploading = false;
    }
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
        <div slot="headline">Product Images</div>

        ${this._images.length > 0 ? html`
          <div class="media-grid">
            ${this._images.map((url, index) => html`
              <div class="media-item ${index === 0 ? 'primary' : ''}">
                <img src="${url}" alt="Product image ${index + 1}" />
                ${index === 0 ? html`<span class="primary-badge">Primary</span>` : ''}
                <div class="media-item-overlay">
                  <div class="media-item-actions">
                    ${index !== 0 ? html`
                      <uui-button
                        compact
                        look="secondary"
                        title="Set as primary"
                        @click=${() => this._handleSetPrimary(index)}
                      >
                        <uui-icon name="icon-star"></uui-icon>
                      </uui-button>
                    ` : ''}
                    <uui-button
                      compact
                      look="secondary"
                      ?disabled=${index === 0}
                      title="Move up"
                      @click=${() => this._handleMoveUp(index)}
                    >
                      <uui-icon name="icon-arrow-up"></uui-icon>
                    </uui-button>
                    <uui-button
                      compact
                      look="secondary"
                      ?disabled=${index === this._images.length - 1}
                      title="Move down"
                      @click=${() => this._handleMoveDown(index)}
                    >
                      <uui-icon name="icon-arrow-down"></uui-icon>
                    </uui-button>
                    <uui-button
                      compact
                      look="secondary"
                      color="danger"
                      title="Remove"
                      @click=${() => this._handleDelete(index)}
                    >
                      <uui-icon name="icon-delete"></uui-icon>
                    </uui-button>
                  </div>
                </div>
              </div>
            `)}
          </div>
        ` : html`
          <div class="empty-state">
            <uui-icon name="icon-picture"></uui-icon>
            <p>No images uploaded yet</p>
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
          <p>${this._uploading ? 'Uploading...' : 'Drag & drop images here or click to upload'}</p>
          <input
            id="fileInput"
            type="file"
            accept="image/*"
            multiple
            @change=${this._handleFileSelect}
          />
        </div>
      </uui-box>

      <h3 class="section-title">Add Image by URL</h3>
      <uui-box>
        <p style="color: var(--uui-color-text-alt); margin-bottom: var(--uui-size-layout-1);">
          You can also add images by providing a URL to an existing image.
        </p>
        <div class="image-url-input">
          <uui-input
            placeholder="https://example.com/image.jpg"
            .value=${this._urlInput}
            @input=${(e) => this._urlInput = e.target.value}
            @keypress=${(e) => e.key === 'Enter' && this._handleAddUrl()}
          ></uui-input>
          <uui-button
            look="primary"
            @click=${this._handleAddUrl}
            ?disabled=${!this._urlInput.trim()}
          >
            Add Image
          </uui-button>
        </div>
      </uui-box>
    `;
  }
}

customElements.define('ecommerce-product-media', ProductMedia);

export default ProductMedia;
