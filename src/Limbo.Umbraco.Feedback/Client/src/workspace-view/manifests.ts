import { UMB_WORKSPACE_CONDITION_ALIAS } from "@umbraco-cms/backoffice/workspace";
import { FEEDBACK_CONDITION_ALIAS, FEEDBACK_WORKSPACE_VIEW_ALIAS } from "../constants.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "workspaceView",
    alias: FEEDBACK_WORKSPACE_VIEW_ALIAS,
    name: "Limbo Feedback Workspace View",
    element: () => import("./feedback-workspace-view.element.js"),
    weight: 100,
    meta: {
      label: "Feedback",
      pathname: "feedback",
      icon: "icon-chat",
    },
    conditions: [
      // Only on documents...
      { alias: UMB_WORKSPACE_CONDITION_ALIAS, match: "Umb.Workspace.Document" },
      // ...and only when the server says the document has (or can have) feedback
      { alias: FEEDBACK_CONDITION_ALIAS },
    ],
  },
];
