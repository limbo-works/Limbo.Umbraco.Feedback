import { manifests as conditions } from "./conditions/manifests.js";
import { manifests as localization } from "./localization/manifests.js";
import { manifests as modals } from "./modals/manifests.js";
import { manifests as workspaceView } from "./workspace-view/manifests.js";

/**
 * All extension manifests of the Limbo Feedback package.
 *
 * Registered through `public/umbraco-package.json`, which replaces the `IManifestFilter` implementation used up
 * to Umbraco 13.
 */
export const manifests: Array<UmbExtensionManifest> = [
  ...localization,
  ...conditions,
  ...modals,
  ...workspaceView,
];
