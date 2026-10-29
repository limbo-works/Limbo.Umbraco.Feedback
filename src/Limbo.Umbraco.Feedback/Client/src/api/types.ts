/**
 * Types mirroring the models of the feedback management API.
 *
 * The Umbraco extension template generates these from `/umbraco/swagger/feedback/swagger.json` with
 * `@hey-api/openapi-ts`. Generating requires a running Umbraco instance, so the handful of models this package
 * needs are declared by hand instead - keep them in sync with `Limbo.Umbraco.Feedback.Models.Api`.
 */

export interface FeedbackWorkspaceView {
  siteKey: string;
  pageKey: string | null;
  icon: string;
}

export interface FeedbackRating {
  alias: string;
  key: string;
  /** Explicit name, or `null` when the back office should localize `feedback_rating{Alias}`. */
  name: string | null;
  active: boolean;
}

export interface FeedbackStatus {
  alias: string;
  key: string;
  /** Explicit name, or `null` when the back office should localize `feedback_status{Alias}`. */
  name: string | null;
  active: boolean;
}

export interface FeedbackSite {
  id: number;
  key: string;
  name: string;
  ratings: Array<FeedbackRating>;
  statuses: Array<FeedbackStatus>;
}

export interface FeedbackPage {
  id: number;
  key: string;
  name: string;
  published: boolean;
  url: string | null;
}

export interface FeedbackUser {
  id: number;
  key: string;
  name: string;
  email: string;
  description: string | null;
  avatar: string | null;
  language: string;
}

export interface FeedbackEntry {
  id: number;
  key: string;
  site: FeedbackSite;
  page: FeedbackPage | null;
  name: string | null;
  email: string | null;
  comment: string | null;
  status: FeedbackStatus | null;
  rating: FeedbackRating | null;
  assignedTo: FeedbackUser | null;
  createDate: string;
  updateDate: string;
  archived: boolean;
}

export interface FeedbackPagination {
  page: number;
  pages: number;
  limit: number;
  total: number;
  offset: number;
}

export interface FeedbackSorting {
  field: "createDate" | "rating" | "status";
  order: "asc" | "desc";
}

export interface FeedbackEntryList {
  pagination: FeedbackPagination;
  sorting: FeedbackSorting;
  data: Array<FeedbackEntry>;
}

export interface FeedbackResult {
  site: FeedbackSite;
  entries: FeedbackEntryList;
}

export interface FeedbackQuery {
  [key: string]: unknown;
  page?: number;
  sort?: string;
  order?: string;
  rating?: string;
  responsible?: string;
  status?: string;
  type?: string;
}
