import { UmbModalToken } from "@umbraco-cms/backoffice/modal";
import {
  FEEDBACK_SELECT_RESPONSIBLE_MODAL_ALIAS,
  FEEDBACK_SELECT_STATUS_MODAL_ALIAS,
} from "../constants.js";
import type { FeedbackStatus, FeedbackUser } from "../api/types.js";

export interface FeedbackSelectStatusModalData {
  statuses: Array<FeedbackStatus>;
  selected?: string;
}

export interface FeedbackSelectStatusModalValue {
  status: string;
}

/** Replaces the `SelectStatus.html` editor view opened through the AngularJS `editorService`. */
export const FEEDBACK_SELECT_STATUS_MODAL = new UmbModalToken<
  FeedbackSelectStatusModalData,
  FeedbackSelectStatusModalValue
>(FEEDBACK_SELECT_STATUS_MODAL_ALIAS, {
  modal: { type: "sidebar", size: "small" },
});

export interface FeedbackSelectResponsibleModalData {
  users: Array<FeedbackUser>;
  selected?: string;
}

export interface FeedbackSelectResponsibleModalValue {
  responsible: string;
}

/** Replaces the `SelectResponsible.html` editor view. */
export const FEEDBACK_SELECT_RESPONSIBLE_MODAL = new UmbModalToken<
  FeedbackSelectResponsibleModalData,
  FeedbackSelectResponsibleModalValue
>(FEEDBACK_SELECT_RESPONSIBLE_MODAL_ALIAS, {
  modal: { type: "sidebar", size: "small" },
});
