/**
 * Repository for the feedback management API.
 *
 * All requests go through `umbHttpClient`, which is pre-configured with the back office base URL and bearer
 * token. Raw `fetch()` would return 401 - see the `umbraco-openapi-client` guidance.
 *
 * The repository is a controller, so it is hosted by (and torn down with) the element that uses it.
 */

import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { umbHttpClient } from "@umbraco-cms/backoffice/http-client";
import { FEEDBACK_API_BASE } from "../constants.js";
import type {
  FeedbackEntry,
  FeedbackQuery,
  FeedbackResult,
  FeedbackUser,
  FeedbackWorkspaceView,
} from "./types.js";

/**
 * Tells the client to attach the bearer token. Generated SDK functions carry this metadata; hand-written calls
 * have to pass it explicitly.
 */
const SECURITY = [{ type: "http", scheme: "bearer" }] as const;

function url(path: string): string {
  return `${FEEDBACK_API_BASE}/${path}`;
}

/**
 * Returns the feedback workspace view for a document, or `undefined` when feedback doesn't apply to it.
 *
 * Exported as a plain function as well, because the workspace view condition needs it before any element (and
 * therefore any repository host) exists.
 *
 * [CHANGE: `umbHttpClient` is created with `throwOnError: true`, so the expected 404 (and any transient failure)
 * rejects rather than resolving with `data: undefined`. Swallow it here so the documented contract holds.]
 * Related: workspace-view/feedback-workspace-view.element.ts, ../../Limbo.Umbraco.Feedback.csproj
 */
export async function getWorkspaceView(unique: string): Promise<FeedbackWorkspaceView | undefined> {
  try {
    const { data } = await umbHttpClient.get<FeedbackWorkspaceView>({
      url: url(`workspace-view/${unique}`),
      security: SECURITY,
    });
    return data ?? undefined;
  } catch {
    return undefined;
  }
}

export class UmbFeedbackRepository extends UmbControllerBase {
  getWorkspaceView(unique: string): Promise<FeedbackWorkspaceView | undefined> {
    return getWorkspaceView(unique);
  }

  async getEntriesForSite(siteKey: string, query: FeedbackQuery): Promise<FeedbackResult | undefined> {
    const { data } = await umbHttpClient.get<FeedbackResult>({
      url: url(`entries/site/${siteKey}`),
      query,
      security: SECURITY,
    });
    return data ?? undefined;
  }

  async getEntriesForPage(pageKey: string, query: FeedbackQuery): Promise<FeedbackResult | undefined> {
    const { data } = await umbHttpClient.get<FeedbackResult>({
      url: url(`entries/page/${pageKey}`),
      query,
      security: SECURITY,
    });
    return data ?? undefined;
  }

  async getUsers(): Promise<Array<FeedbackUser>> {
    const { data } = await umbHttpClient.get<Array<FeedbackUser>>({
      url: url("users"),
      security: SECURITY,
    });
    return data ?? [];
  }

  async setStatus(entryKey: string, status: string): Promise<FeedbackEntry | undefined> {
    const { data } = await umbHttpClient.post<FeedbackEntry>({
      url: url(`entries/${entryKey}/status`),
      body: { status },
      security: SECURITY,
    });
    return data ?? undefined;
  }

  async setResponsible(entryKey: string, responsible: string): Promise<FeedbackEntry | undefined> {
    const { data } = await umbHttpClient.post<FeedbackEntry>({
      url: url(`entries/${entryKey}/responsible`),
      body: { responsible },
      security: SECURITY,
    });
    return data ?? undefined;
  }

  // The client throws on a non-2xx response, so these have to catch rather than inspect `error` - otherwise a
  // failed archive/delete rejects and the caller's "unable to archive/delete" notification is never reached.
  async archive(entryKey: string): Promise<boolean> {
    try {
      await umbHttpClient.post({
        url: url(`entries/${entryKey}/archive`),
        security: SECURITY,
      });
      return true;
    } catch {
      return false;
    }
  }

  async delete(entryKey: string): Promise<boolean> {
    try {
      await umbHttpClient.delete({
        url: url(`entries/${entryKey}`),
        security: SECURITY,
      });
      return true;
    } catch {
      return false;
    }
  }
}
