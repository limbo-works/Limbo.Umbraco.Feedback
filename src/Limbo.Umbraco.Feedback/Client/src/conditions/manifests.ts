import { FEEDBACK_CONDITION_ALIAS } from "../constants.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "condition",
    name: "Limbo Feedback Has Feedback Condition",
    alias: FEEDBACK_CONDITION_ALIAS,
    api: () => import("./has-feedback.condition.js"),
  },
];
