export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "localization",
    alias: "Limbo.Feedback.Localization.EnUs",
    name: "Limbo Feedback English (US)",
    meta: { culture: "en-us" },
    js: () => import("./files/en-us.js"),
  },
  {
    type: "localization",
    alias: "Limbo.Feedback.Localization.DaDk",
    name: "Limbo Feedback Danish",
    meta: { culture: "da-dk" },
    js: () => import("./files/da-dk.js"),
  },
];
