import { css, customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { statusName } from "../utils/format.js";
import type {
  FeedbackSelectStatusModalData,
  FeedbackSelectStatusModalValue,
} from "./tokens.js";

@customElement("limbo-feedback-select-status-modal")
export class LimboFeedbackSelectStatusModalElement extends UmbModalBaseElement<
  FeedbackSelectStatusModalData,
  FeedbackSelectStatusModalValue
> {
  @state()
  private _selected?: string;

  override connectedCallback() {
    super.connectedCallback();
    this._selected = this.data?.selected ?? this.data?.statuses[0]?.key;
  }

  #select(key: string) {
    this._selected = key;
    this.updateValue({ status: key });
  }

  #submit() {
    if (!this._selected) return;
    this.updateValue({ status: this._selected });
    this._submitModal();
  }

  override render() {
    return html`
      <umb-body-layout headline=${this.localize.term("feedback_btnSelectStatus")}>
        <uui-box>
          <div class="list">
            ${this.data?.statuses.map(
              (status) => html`
                <button
                  type="button"
                  class="item ${this._selected === status.key ? "selected" : ""}"
                  @click=${() => this.#select(status.key)}
                >
                  <uui-icon name=${this._selected === status.key ? "icon-check" : "icon-circle-dotted"}></uui-icon>
                  <span>${statusName(this.localize, status)}</span>
                </button>
              `,
            )}
          </div>
        </uui-box>
        <div slot="actions">
          <uui-button
            label=${this.localize.term("general_close")}
            @click=${() => this._rejectModal()}
          ></uui-button>
          <uui-button
            look="primary"
            color="positive"
            label=${this.localize.term("general_submit")}
            ?disabled=${!this._selected}
            @click=${this.#submit}
          ></uui-button>
        </div>
      </umb-body-layout>
    `;
  }

  static override styles = [
    UmbTextStyles,
    css`
      .list {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-1);
      }

      .item {
        display: flex;
        align-items: center;
        gap: var(--uui-size-space-3);
        padding: var(--uui-size-space-3);
        border: 1px solid var(--uui-color-border);
        border-radius: var(--uui-border-radius);
        background: none;
        font: inherit;
        color: inherit;
        cursor: pointer;
        text-align: left;
      }

      .item:hover {
        border-color: var(--uui-color-border-emphasis);
      }

      .item.selected {
        border-color: var(--uui-color-selected);
        background-color: var(--uui-color-selected-contrast);
      }
    `,
  ];
}

export { LimboFeedbackSelectStatusModalElement as element };

declare global {
  interface HTMLElementTagNameMap {
    "limbo-feedback-select-status-modal": LimboFeedbackSelectStatusModalElement;
  }
}
