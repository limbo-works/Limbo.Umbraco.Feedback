import { css, customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { EMPTY_GUID } from "../constants.js";
import { initials } from "../utils/format.js";
import type {
  FeedbackSelectResponsibleModalData,
  FeedbackSelectResponsibleModalValue,
} from "./tokens.js";

@customElement("limbo-feedback-select-responsible-modal")
export class LimboFeedbackSelectResponsibleModalElement extends UmbModalBaseElement<
  FeedbackSelectResponsibleModalData,
  FeedbackSelectResponsibleModalValue
> {
  @state()
  private _selected: string = EMPTY_GUID;

  override connectedCallback() {
    super.connectedCallback();
    this._selected = this.data?.selected ?? EMPTY_GUID;
  }

  #select(key: string) {
    this._selected = key;
    this.updateValue({ responsible: key });
  }

  #submit() {
    this.updateValue({ responsible: this._selected });
    this._submitModal();
  }

  override render() {
    return html`
      <umb-body-layout headline=${this.localize.term("feedback_btnSelectResponsible")}>
        <uui-box>
          <div class="list">
            <button
              type="button"
              class="item ${this._selected === EMPTY_GUID ? "selected" : ""}"
              @click=${() => this.#select(EMPTY_GUID)}
            >
              <span class="avatar"><uui-icon name="icon-user"></uui-icon></span>
              <em>${this.localize.term("feedback_labelNoResponsible")}</em>
            </button>
            ${this.data?.users.map(
              (user) => html`
                <button
                  type="button"
                  class="item ${this._selected === user.key ? "selected" : ""}"
                  @click=${() => this.#select(user.key)}
                >
                  <span
                    class="avatar"
                    style=${user.avatar ? `background-image: url(${user.avatar})` : ""}
                  >
                    ${user.avatar ? "" : initials(user.name)}
                  </span>
                  <span>
                    ${user.name}
                    ${user.description ? html`<small>(${user.description})</small>` : ""}
                  </span>
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

      .avatar {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex: 0 0 auto;
        width: 32px;
        height: 32px;
        border-radius: 50%;
        background-color: var(--uui-color-surface-alt);
        background-size: cover;
        background-position: center;
        font-size: 12px;
      }

      small {
        color: var(--uui-color-text-alt);
      }
    `,
  ];
}

export { LimboFeedbackSelectResponsibleModalElement as element };

declare global {
  interface HTMLElementTagNameMap {
    "limbo-feedback-select-responsible-modal": LimboFeedbackSelectResponsibleModalElement;
  }
}
