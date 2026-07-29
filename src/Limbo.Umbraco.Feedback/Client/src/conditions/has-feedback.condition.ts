import { UmbConditionBase } from "@umbraco-cms/backoffice/extension-registry";
import { UMB_ENTITY_CONTEXT } from "@umbraco-cms/backoffice/entity";
import type {
  UmbConditionConfigBase,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from "@umbraco-cms/backoffice/extension-api";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { getWorkspaceView } from "../api/feedback.repository.js";

/**
 * Condition deciding whether the feedback workspace view should be shown for the current document.
 *
 * Umbraco 13 made this decision on the server, by returning a `ContentApp` from `IFeedbackPlugin`. Content apps
 * were removed in Umbraco 14, so the decision is now asked for over HTTP - the server still owns the logic (which
 * content types are sites/pages, and whether a plugin opts out), this condition just relays the answer.
 */
export class UmbFeedbackHasFeedbackCondition
  extends UmbConditionBase<UmbConditionConfigBase>
  implements UmbExtensionCondition
{
  constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
    super(host, args);

    this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
      this.observe(
        context?.unique,
        async (unique) => {
          if (!unique) {
            this.permitted = false;
            return;
          }

          try {
            this.permitted = !!(await getWorkspaceView(unique));
          } catch {
            // A missing/failed lookup simply means no feedback view - never block the workspace
            this.permitted = false;
          }
        },
        "limboFeedbackConditionUnique",
      );
    });
  }
}

export { UmbFeedbackHasFeedbackCondition as api };
