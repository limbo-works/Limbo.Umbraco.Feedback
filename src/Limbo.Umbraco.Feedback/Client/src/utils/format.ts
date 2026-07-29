import type { UmbLocalizationController } from "@umbraco-cms/backoffice/localization-api";
import type { FeedbackRating, FeedbackStatus } from "../api/types.js";

/**
 * Returns the display name of a rating.
 *
 * Ratings without an explicit name fall back to the localized `feedback_rating{Alias}` key. Until Umbraco 14 this
 * fallback happened server side through `ILocalizedTextService`; package language files are no longer loaded, so
 * it now happens here against the `localization` extension manifests.
 */
export function ratingName(localize: UmbLocalizationController, rating?: FeedbackRating | null): string {
  if (!rating) return "";
  return rating.name ?? localize.term(`feedback_rating${pascal(rating.alias)}`);
}

/** Returns the display name of a status - see {@link ratingName}. */
export function statusName(localize: UmbLocalizationController, status?: FeedbackStatus | null): string {
  if (!status) return "";
  return status.name ?? localize.term(`feedback_status${pascal(status.alias)}`);
}

/**
 * Pascal cases an alias, matching the `ToPascalCase()` the Umbraco 13 server used when it built the same keys.
 *
 * [CHANGE: only upper casing the first character produced `feedback_ratingVery-happy` for a custom rating with a
 * hyphenated alias, which never matches a localization key.] Related: api/feedback.repository.ts,
 * workspace-view/feedback-workspace-view.element.ts
 */
function pascal(alias: string): string {
  return alias
    .replace(/[^a-zA-Z0-9]+([a-zA-Z0-9])?/g, (_, c: string | undefined) => (c ? c.toUpperCase() : ""))
    .replace(/^[a-z]/, (c) => c.toUpperCase());
}

/** Formats an ISO date as an absolute date/time in the current locale. */
export function formatDate(value: string): string {
  const date = toDate(value);
  return date ? date.toLocaleString() : "";
}

/**
 * Formats an ISO date as a relative time ("2 hours ago").
 *
 * Replaces the `createDateDiff` / `updateDateDiff` strings the Umbraco 13 API rendered server side.
 */
export function formatRelativeDate(value: string): string {
  const date = toDate(value);
  if (!date) return "";

  const seconds = Math.round((date.getTime() - Date.now()) / 1000);
  const format = new Intl.RelativeTimeFormat(undefined, { numeric: "auto" });

  const units: Array<[Intl.RelativeTimeFormatUnit, number]> = [
    ["year", 60 * 60 * 24 * 365],
    ["month", 60 * 60 * 24 * 30],
    ["day", 60 * 60 * 24],
    ["hour", 60 * 60],
    ["minute", 60],
  ];

  for (const [unit, size] of units) {
    if (Math.abs(seconds) >= size) return format.format(Math.round(seconds / size), unit);
  }

  return format.format(seconds, "second");
}

/**
 * Dates are serialized without a time zone designator, but are UTC - the same assumption the Umbraco 13 back
 * office made.
 */
function toDate(value: string): Date | undefined {
  if (!value) return undefined;
  const normalized = /(Z|[+-]\d{2}:?\d{2})$/.test(value) ? value : `${value}Z`;
  const date = new Date(normalized);
  return isNaN(date.getTime()) ? undefined : date;
}

/** Returns the initials of a user name, used for the avatar fallback. */
export function initials(name: string): string {
  return name
    .split(" ")
    .map((x) => x.charAt(0))
    .slice(0, 2)
    .join("")
    .toUpperCase();
}
