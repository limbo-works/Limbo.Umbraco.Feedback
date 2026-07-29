/**
 * English (US) localization.
 *
 * Ported from `~/wwwroot/Lang/en-US.xml`. Umbraco no longer loads package language files from disk, so
 * translations are registered as `localization` extension manifests instead. Keys keep the `feedback_` prefix
 * they had before, so `feedback_labelRating` still resolves the same string.
 */
export default {
  feedback: {
    // Ratings and statuses
    statusNew: "New",
    statusInprogress: "In progress",
    statusInProgress: "In progress",
    statusClosed: "Closed",
    statusNotFound: "Status not found",
    ratingPositive: "Positive",
    ratingNegative: "Negative",
    ratingNotFound: "Rating not found",

    // Labels
    labelId: "ID",
    labelSite: "Site",
    labelPage: "Page",
    labelPageId: "Page ID",
    labelPageTitle: "Page Title",
    labelPageUrl: "URL",
    labelAssignedTo: "Assigned to",
    labelResponsible: "Responsible",
    labelRating: "Rating",
    labelName: "Name",
    labelEmail: "E-mail",
    labelAdded: "Added",
    labelUpdateDate: "Updated",
    labelStatus: "Status",
    labelComment: "Comment",
    labelNoResponsible: "No responsible",
    labelNotSpecified: "Not specified",
    labelYes: "Yes",
    labelNo: "No",
    labelRefresh: "Refresh",

    // Buttons
    btnArchive: "Archive",
    btnDelete: "Delete",
    btnCleaning: "Cleaning",
    btnSelectStatus: "Select status",
    btnSelectResponsible: "Select responsible",
    btnShowInUmbraco: "Show in Umbraco",
    btnShowAtWebsite: "Show at website",

    // Filters
    labelAllRatings: "All ratings",
    labelAllUsers: "All users",
    labelMe: "Me",
    labelAllStatuses: "All statuses",
    labelAllTypes: "All types",
    labelOnlyWithRating: "Only with rating",
    labelRatingAndComment: "Rating and comment",

    // Messages
    noEntriesForSite: "This site currently has no feedback entries.",
    noEntriesForPage: "This page currently has no feedback entries.",
    noEntriesForFilter: "No feedback entries match your filters.",
    pageNoLongerExists: "The page no longer exists",
    confirmArchive: "Are you sure you want to archive the selected feedback entry?",
    confirmDelete: "Are you sure you want to delete the selected feedback entry?",
    archiveSuccess: "The entry was successfully archived.",
    archiveError: "Unable to archive the entry.",
    deleteSuccess: "The entry was successfully deleted.",
    deleteError: "Unable to delete the entry.",
    loadError: "Unable to load the feedback entries.",
    saveError: "Unable to save the entry.",
  },
};
