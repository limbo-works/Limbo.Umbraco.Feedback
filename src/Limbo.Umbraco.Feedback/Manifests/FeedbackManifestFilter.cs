using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace Limbo.Umbraco.Feedback.Manifests {

    /// <inheritdoc />
    public class FeedbackManifestFilter : IManifestFilter {

        /// <inheritdoc />
        public void Filter(List<PackageManifest> manifests) {

            // Initialize a new manifest filter for this package
            PackageManifest manifest = new() {
                AllowPackageTelemetry = true,
                PackageName = FeedbackPackage.Name,
                Version = FeedbackPackage.InformationalVersion,
                BundleOptions = BundleOptions.Independent,
                Scripts = new[] {
                    $"/App_Plugins/{FeedbackPackage.Alias}/Scripts/Controllers/ContentApp.js",
                    $"/App_Plugins/{FeedbackPackage.Alias}/Scripts/Controllers/ContentAppPage.js",
                    $"/App_Plugins/{FeedbackPackage.Alias}/Scripts/Controllers/SelectStatus.js",
                    $"/App_Plugins/{FeedbackPackage.Alias}/Scripts/Controllers/SelectResponsible.js"
                },
                Stylesheets = new[] {
                    $"/App_Plugins/{FeedbackPackage.Alias}/Styles/Default.css"
                }
            };

            manifests.Add(manifest);

        }

    }

}