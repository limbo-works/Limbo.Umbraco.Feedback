/**
 * The feedback workspace view.
 *
 * This is the Umbraco 17 replacement for the two AngularJS content apps (`ContentApp.html` for sites and
 * `ContentAppPage.html` for pages). Both are now one element - the server tells us through the workspace-view
 * endpoint whether we are looking at a site or a single page, and the page column is hidden in the latter case.
 */

import { css, customElement, html, nothing, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UMB_ENTITY_CONTEXT } from "@umbraco-cms/backoffice/entity";
import { UMB_MODAL_MANAGER_CONTEXT, umbConfirmModal } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import type { UmbModalManagerContext } from "@umbraco-cms/backoffice/modal";
import type { UmbNotificationContext } from "@umbraco-cms/backoffice/notification";

import { UmbFeedbackRepository } from "../api/feedback.repository.js";
import type {
  FeedbackEntry,
  FeedbackQuery,
  FeedbackResult,
  FeedbackUser,
  FeedbackWorkspaceView,
} from "../api/types.js";
import { EMPTY_GUID } from "../constants.js";
import {
  FEEDBACK_SELECT_RESPONSIBLE_MODAL,
  FEEDBACK_SELECT_STATUS_MODAL,
} from "../modals/tokens.js";
import { formatDate, formatRelativeDate, ratingName, statusName } from "../utils/format.js";

type Filters = {
  rating: string;
  responsible: string;
  status: string;
  type: string;
};

@customElement("limbo-feedback-workspace-view")
export class LimboFeedbackWorkspaceViewElement extends UmbLitElement {
  #repository = new UmbFeedbackRepository(this);
  #modalManager?: UmbModalManagerContext;
  #notifications?: UmbNotificationContext;

  @state() private _unique?: string;
  @state() private _view?: FeedbackWorkspaceView;
  @state() private _result?: FeedbackResult;
  @state() private _users: Array<FeedbackUser> = [];
  @state() private _loading = true;
  @state() private _expanded = new Set<string>();
  @state() private _page = 1;
  @state() private _sort = "createDate";
  @state() private _order = "desc";
  @state() private _filters: Filters = { rating: "", responsible: "", status: "", type: "" };

  constructor() {
    super();

    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
      this.#modalManager = context ?? undefined;
    });

    this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
      this.#notifications = context ?? undefined;
    });

    this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
      this.observe(
        context?.unique,
        (unique) => {
          if (!unique || unique === this._unique) return;
          this._unique = unique;
          void this.#init();
        },
        "limboFeedbackViewUnique",
      );
    });
  }

  // ------------------------------------------------------------------ loading

  // [CHANGE: the HTTP client rejects on non-2xx, so every await here needs guarding - an unguarded rejection
  // left `_loading` true and the view stuck on a spinner. `#load` additionally drops out-of-order responses.]
  // Related: api/feedback.repository.ts, utils/format.ts, ../../Limbo.Umbraco.Feedback.csproj
  async #init() {
    if (!this._unique) return;

    this._loading = true;

    try {
      this._view = await this.#repository.getWorkspaceView(this._unique);
      if (!this._view) {
        this._loading = false;
        return;
      }

      this._users = await this.#repository.getUsers();
    } catch {
      this._loading = false;
      this.#error("feedback_loadError");
      return;
    }

    await this.#load();
  }

  /** Incremented for every request, so a slow earlier response can't overwrite a newer one. */
  #loadToken = 0;

  async #load() {
    if (!this._view) return;

    const token = ++this.#loadToken;

    this._loading = true;

    const query: FeedbackQuery = { page: this._page, sort: this._sort, order: this._order };
    if (this._filters.rating) query.rating = this._filters.rating;
    if (this._filters.responsible) query.responsible = this._filters.responsible;
    if (this._filters.status) query.status = this._filters.status;
    if (this._filters.type) query.type = this._filters.type;

    try {
      const result = this._view.pageKey
        ? await this.#repository.getEntriesForPage(this._view.pageKey, query)
        : await this.#repository.getEntriesForSite(this._view.siteKey, query);

      if (token !== this.#loadToken) return;

      this._result = result;

      // Deleting or archiving the last entry of the last page can leave us past the end of the result set,
      // which would otherwise render as "no entries" even though the site still has feedback
      const pages = result?.entries.pagination.pages ?? 0;
      if (pages > 0 && this._page > pages) {
        this._page = pages;
        await this.#load();
        return;
      }
    } catch {
      if (token === this.#loadToken) this.#error("feedback_loadError");
    } finally {
      if (token === this.#loadToken) this._loading = false;
    }
  }

  // ----------------------------------------------------------------- commands

  #sortBy(field: string) {
    if (this._sort === field) {
      this._order = this._order === "asc" ? "desc" : "asc";
    } else {
      this._sort = field;
      this._order = field === "createDate" ? "desc" : "asc";
    }
    this._page = 1;
    void this.#load();
  }

  #setFilter(key: keyof Filters, value: string) {
    this._filters = { ...this._filters, [key]: value };
    this._page = 1;
    void this.#load();
  }

  #goToPage(page: number) {
    this._page = page;
    void this.#load();
  }

  #toggle(key: string) {
    const expanded = new Set(this._expanded);
    if (expanded.has(key)) {
      expanded.delete(key);
    } else {
      expanded.add(key);
    }
    this._expanded = expanded;
  }

  async #archive(entry: FeedbackEntry) {
    await umbConfirmModal(this, {
      headline: this.localize.term("feedback_btnArchive"),
      content: this.localize.term("feedback_confirmArchive"),
      confirmLabel: this.localize.term("feedback_btnArchive"),
      color: "warning",
    });

    if (await this.#repository.archive(entry.key)) {
      this.#success("feedback_archiveSuccess");
      await this.#load();
    } else {
      this.#error("feedback_archiveError");
    }
  }

  async #delete(entry: FeedbackEntry) {
    await umbConfirmModal(this, {
      headline: this.localize.term("feedback_btnDelete"),
      content: this.localize.term("feedback_confirmDelete"),
      confirmLabel: this.localize.term("feedback_btnDelete"),
      color: "danger",
    });

    if (await this.#repository.delete(entry.key)) {
      this.#success("feedback_deleteSuccess");
      await this.#load();
    } else {
      this.#error("feedback_deleteError");
    }
  }

  async #openSelectStatus(entry: FeedbackEntry) {
    const modal = this.#modalManager?.open(this, FEEDBACK_SELECT_STATUS_MODAL, {
      data: { statuses: entry.site.statuses, selected: entry.status?.key },
    });
    if (!modal) return;

    const value = await modal.onSubmit().catch(() => undefined);
    if (!value) return;

    try {
      await this.#repository.setStatus(entry.key, value.status);
      await this.#load();
    } catch {
      this.#error("feedback_saveError");
    }
  }

  async #openSelectResponsible(entry: FeedbackEntry) {
    const modal = this.#modalManager?.open(this, FEEDBACK_SELECT_RESPONSIBLE_MODAL, {
      data: { users: this._users, selected: entry.assignedTo?.key ?? EMPTY_GUID },
    });
    if (!modal) return;

    const value = await modal.onSubmit().catch(() => undefined);
    if (!value) return;

    try {
      await this.#repository.setResponsible(entry.key, value.responsible);
      await this.#load();
    } catch {
      this.#error("feedback_saveError");
    }
  }

  #success(key: string) {
    this.#notifications?.peek("positive", { data: { message: this.localize.term(key) } });
  }

  #error(key: string) {
    this.#notifications?.peek("danger", { data: { message: this.localize.term(key) } });
  }

  // ------------------------------------------------------------------ render

  override render() {
    if (!this._view) {
      return this._loading ? html`<uui-loader></uui-loader>` : nothing;
    }

    return html`
      <uui-box>
        ${this.#renderToolbar()} ${this.#renderTable()} ${this.#renderPagination()}
        ${this._loading ? html`<uui-loader-bar></uui-loader-bar>` : nothing}
      </uui-box>
    `;
  }

  #renderToolbar() {
    const site = this._result?.site;

    return html`
      <div class="toolbar">
        <div class="filters">
          <uui-select
            label=${this.localize.term("feedback_labelRating")}
            .options=${[
              { name: this.localize.term("feedback_labelAllRatings"), value: "", selected: !this._filters.rating },
              ...(site?.ratings ?? []).map((x) => ({
                name: ratingName(this.localize, x),
                value: x.key,
                selected: this._filters.rating === x.key,
              })),
            ]}
            @change=${(e: Event) => this.#setFilter("rating", (e.target as HTMLInputElement).value)}
          ></uui-select>

          <uui-select
            label=${this.localize.term("feedback_labelResponsible")}
            .options=${[
              { name: this.localize.term("feedback_labelAllUsers"), value: "", selected: !this._filters.responsible },
              {
                name: this.localize.term("feedback_labelNoResponsible"),
                value: EMPTY_GUID,
                selected: this._filters.responsible === EMPTY_GUID,
              },
              ...this._users.map((x) => ({
                name: x.name,
                value: x.key,
                selected: this._filters.responsible === x.key,
              })),
            ]}
            @change=${(e: Event) => this.#setFilter("responsible", (e.target as HTMLInputElement).value)}
          ></uui-select>

          <uui-select
            label=${this.localize.term("feedback_labelStatus")}
            .options=${[
              { name: this.localize.term("feedback_labelAllStatuses"), value: "", selected: !this._filters.status },
              ...(site?.statuses ?? []).map((x) => ({
                name: statusName(this.localize, x),
                value: x.key,
                selected: this._filters.status === x.key,
              })),
            ]}
            @change=${(e: Event) => this.#setFilter("status", (e.target as HTMLInputElement).value)}
          ></uui-select>

          <uui-select
            label=${this.localize.term("feedback_labelAllTypes")}
            .options=${[
              { name: this.localize.term("feedback_labelAllTypes"), value: "", selected: !this._filters.type },
              {
                name: this.localize.term("feedback_labelOnlyWithRating"),
                value: "rating",
                selected: this._filters.type === "rating",
              },
              {
                name: this.localize.term("feedback_labelRatingAndComment"),
                value: "comment",
                selected: this._filters.type === "comment",
              },
            ]}
            @change=${(e: Event) => this.#setFilter("type", (e.target as HTMLInputElement).value)}
          ></uui-select>
        </div>

        <uui-button
          look="outline"
          label=${this.localize.term("feedback_labelRefresh")}
          @click=${() => this.#load()}
        >
          <uui-icon name="icon-refresh"></uui-icon>
          ${this.localize.term("feedback_labelRefresh")}
        </uui-button>
      </div>
    `;
  }

  #renderTable() {
    const entries = this._result?.entries.data ?? [];
    const showPage = !this._view?.pageKey;

    if (!entries.length && !this._loading) {
      const hasFilter = Object.values(this._filters).some((x) => !!x);
      const key = hasFilter
        ? "feedback_noEntriesForFilter"
        : showPage
          ? "feedback_noEntriesForSite"
          : "feedback_noEntriesForPage";
      return html`<p class="empty">${this.localize.term(key)}</p>`;
    }

    return html`
      <uui-table>
        <uui-table-head>
          <uui-table-head-cell>${this.#sortHeader("rating", "feedback_labelRating")}</uui-table-head-cell>
          ${showPage ? html`<uui-table-head-cell>${this.localize.term("feedback_labelPage")}</uui-table-head-cell>` : nothing}
          <uui-table-head-cell>${this.localize.term("feedback_labelResponsible")}</uui-table-head-cell>
          <uui-table-head-cell>${this.localize.term("feedback_labelComment")}</uui-table-head-cell>
          <uui-table-head-cell>${this.#sortHeader("status", "feedback_labelStatus")}</uui-table-head-cell>
          <uui-table-head-cell>${this.#sortHeader("createDate", "feedback_labelAdded")}</uui-table-head-cell>
        </uui-table-head>
        ${repeat(
          entries,
          (entry) => entry.key,
          (entry) => this.#renderRow(entry, showPage),
        )}
      </uui-table>
    `;
  }

  #sortHeader(field: string, labelKey: string) {
    const active = this._sort === field;
    return html`
      <button type="button" class="sort" @click=${() => this.#sortBy(field)}>
        ${this.localize.term(labelKey)}
        ${active
          ? html`<uui-icon name=${this._order === "asc" ? "icon-navigation-up" : "icon-navigation-down"}></uui-icon>`
          : nothing}
      </button>
    `;
  }

  #renderRow(entry: FeedbackEntry, showPage: boolean) {
    const expanded = this._expanded.has(entry.key);
    const columns = showPage ? 6 : 5;

    return html`
      <uui-table-row class="summary" @click=${() => this.#toggle(entry.key)}>
        <uui-table-cell>
          <span class="rating rating-${entry.rating?.alias}">${ratingName(this.localize, entry.rating)}</span>
        </uui-table-cell>
        ${showPage ? html`<uui-table-cell>${entry.page?.name ?? ""}</uui-table-cell>` : nothing}
        <uui-table-cell>
          ${entry.assignedTo
            ? entry.assignedTo.name
            : html`<em>${this.localize.term("feedback_labelNoResponsible")}</em>`}
        </uui-table-cell>
        <uui-table-cell>
          ${this.localize.term(entry.comment ? "feedback_labelYes" : "feedback_labelNo")}
        </uui-table-cell>
        <uui-table-cell>
          <span class="status status-${entry.status?.alias}">${statusName(this.localize, entry.status)}</span>
        </uui-table-cell>
        <uui-table-cell>${formatDate(entry.createDate)}</uui-table-cell>
      </uui-table-row>
      ${expanded
        ? html`
            <uui-table-row class="details">
              <uui-table-cell colspan=${columns}>${this.#renderDetails(entry)}</uui-table-cell>
            </uui-table-row>
          `
        : nothing}
    `;
  }

  #renderDetails(entry: FeedbackEntry) {
    return html`
      <dl>
        <dt>ID</dt>
        <dd>${entry.id}</dd>

        <dt>Key</dt>
        <dd><code>${entry.key}</code></dd>

        <dt>${this.localize.term("feedback_labelPageTitle")}</dt>
        <dd>${entry.page?.name ?? html`<em>${this.localize.term("feedback_pageNoLongerExists")}</em>`}</dd>

        <dt>${this.localize.term("feedback_labelPageUrl")}</dt>
        <dd>${entry.page?.url ?? html`<em>${this.localize.term("feedback_pageNoLongerExists")}</em>`}</dd>

        <dt>${this.localize.term("feedback_labelAssignedTo")}</dt>
        <dd>
          ${entry.assignedTo
            ? html`${entry.assignedTo.name}
              ${entry.assignedTo.description ? html`<small>(${entry.assignedTo.description})</small>` : nothing}`
            : html`<em>${this.localize.term("feedback_labelNoResponsible")}</em>`}
        </dd>

        <dt>${this.localize.term("feedback_labelName")}</dt>
        <dd>${entry.name ?? html`<em>${this.localize.term("feedback_labelNotSpecified")}</em>`}</dd>

        <dt>${this.localize.term("feedback_labelEmail")}</dt>
        <dd>
          ${entry.email
            ? html`<a href="mailto:${entry.email}">${entry.email}</a>`
            : html`<em>${this.localize.term("feedback_labelNotSpecified")}</em>`}
        </dd>

        <dt>${this.localize.term("feedback_labelAdded")}</dt>
        <dd>${formatDate(entry.createDate)} <small>(${formatRelativeDate(entry.createDate)})</small></dd>

        <dt>${this.localize.term("feedback_labelUpdateDate")}</dt>
        <dd>${formatDate(entry.updateDate)} <small>(${formatRelativeDate(entry.updateDate)})</small></dd>

        <dt>${this.localize.term("feedback_labelComment")}</dt>
        <dd class="comment">
          ${entry.comment ?? html`<em>${this.localize.term("feedback_labelNotSpecified")}</em>`}
        </dd>
      </dl>

      <div class="actions">
        <uui-button
          look="outline"
          label=${this.localize.term("feedback_btnArchive")}
          @click=${() => void this.#archive(entry).catch(() => undefined)}
        ></uui-button>
        <uui-button
          look="outline"
          color="danger"
          label=${this.localize.term("feedback_btnDelete")}
          @click=${() => void this.#delete(entry).catch(() => undefined)}
        ></uui-button>
        <uui-button
          look="outline"
          label=${this.localize.term("feedback_btnSelectStatus")}
          @click=${() => this.#openSelectStatus(entry)}
        ></uui-button>
        <uui-button
          look="outline"
          label=${this.localize.term("feedback_btnSelectResponsible")}
          @click=${() => this.#openSelectResponsible(entry)}
        ></uui-button>
        ${entry.page
          ? html`
              <uui-button
                look="outline"
                label=${this.localize.term("feedback_btnShowInUmbraco")}
                href="/umbraco/section/content/workspace/document/edit/${entry.page.key}"
              ></uui-button>
            `
          : nothing}
        ${entry.page?.url
          ? html`
              <uui-button
                look="outline"
                label=${this.localize.term("feedback_btnShowAtWebsite")}
                href=${entry.page.url}
                target="_blank"
              ></uui-button>
            `
          : nothing}
      </div>
    `;
  }

  #renderPagination() {
    const pagination = this._result?.entries.pagination;
    if (!pagination || pagination.pages <= 1) return nothing;

    return html`
      <div class="pagination">
        <uui-pagination
          .current=${pagination.page}
          .total=${pagination.pages}
          @change=${(e: Event) => this.#goToPage((e.target as unknown as { current: number }).current)}
        ></uui-pagination>
      </div>
    `;
  }

  static override styles = [
    UmbTextStyles,
    css`
      :host {
        display: block;
        padding: var(--uui-size-layout-1);
      }

      .toolbar {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        align-items: flex-end;
        gap: var(--uui-size-space-3);
        margin-bottom: var(--uui-size-space-4);
      }

      .filters {
        display: flex;
        flex-wrap: wrap;
        gap: var(--uui-size-space-3);
      }

      .sort {
        display: inline-flex;
        align-items: center;
        gap: var(--uui-size-space-1);
        background: none;
        border: none;
        padding: 0;
        font: inherit;
        color: inherit;
        cursor: pointer;
      }

      .summary {
        cursor: pointer;
      }

      .rating,
      .status {
        display: inline-block;
        padding: 2px 8px;
        border-radius: var(--uui-border-radius);
        background-color: var(--uui-color-surface-alt);
        white-space: nowrap;
      }

      .rating-positive {
        background-color: var(--uui-color-positive);
        color: var(--uui-color-positive-contrast);
      }

      .rating-negative {
        background-color: var(--uui-color-danger);
        color: var(--uui-color-danger-contrast);
      }

      dl {
        display: grid;
        grid-template-columns: minmax(120px, max-content) 1fr;
        gap: var(--uui-size-space-2) var(--uui-size-space-4);
        margin: 0 0 var(--uui-size-space-4);
      }

      dt {
        font-weight: bold;
      }

      dd {
        margin: 0;
      }

      dd.comment {
        white-space: pre-wrap;
      }

      .actions {
        display: flex;
        flex-wrap: wrap;
        gap: var(--uui-size-space-2);
      }

      .pagination {
        display: flex;
        justify-content: center;
        margin-top: var(--uui-size-space-4);
      }

      .empty {
        color: var(--uui-color-text-alt);
      }

      small {
        color: var(--uui-color-text-alt);
      }
    `,
  ];
}

export default LimboFeedbackWorkspaceViewElement;

declare global {
  interface HTMLElementTagNameMap {
    "limbo-feedback-workspace-view": LimboFeedbackWorkspaceViewElement;
  }
}
