import {
  FEEDBACK_SELECT_RESPONSIBLE_MODAL_ALIAS,
  FEEDBACK_SELECT_STATUS_MODAL_ALIAS,
} from "../constants.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "modal",
    alias: FEEDBACK_SELECT_STATUS_MODAL_ALIAS,
    name: "Limbo Feedback Select Status Modal",
    element: () => import("./select-status.modal.element.js"),
  },
  {
    type: "modal",
    alias: FEEDBACK_SELECT_RESPONSIBLE_MODAL_ALIAS,
    name: "Limbo Feedback Select Responsible Modal",
    element: () => import("./select-responsible.modal.element.js"),
  },
];
