/**
 * Shared constants for the Limbo Feedback back office extension.
 */

/**
 * Base URL of the feedback management API.
 *
 * Matches `FeedbackConstants.ApiRoute` on the server, prefixed with Umbraco's back office path. Change both if
 * the back office path is customised through `Umbraco:CMS:Global:UmbracoPath`.
 */
export const FEEDBACK_API_BASE = "/umbraco/feedback/api/v1";

export const FEEDBACK_WORKSPACE_VIEW_ALIAS = "Limbo.Feedback.WorkspaceView";
export const FEEDBACK_CONDITION_ALIAS = "Limbo.Feedback.Condition.HasFeedback";
export const FEEDBACK_SELECT_STATUS_MODAL_ALIAS = "Limbo.Feedback.Modal.SelectStatus";
export const FEEDBACK_SELECT_RESPONSIBLE_MODAL_ALIAS = "Limbo.Feedback.Modal.SelectResponsible";

/** Used both as "no responsible" filter value and to clear the assignment of an entry. */
export const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

/** Entries returned per page by the management API. */
export const PER_PAGE = 10;
