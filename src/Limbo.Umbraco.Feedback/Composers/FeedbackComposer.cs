using Limbo.Umbraco.Feedback.Config;
using Limbo.Umbraco.Feedback.ContentApps;
using Limbo.Umbraco.Feedback.Extensions;
using Limbo.Umbraco.Feedback.Manifests;
using Limbo.Umbraco.Feedback.Plugins;
using Limbo.Umbraco.Feedback.Services;
using Microsoft.Extensions.DependencyInjection;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Limbo.Umbraco.Feedback.Composers {

#pragma warning disable 1591

    public class FeedbackComposer : IComposer {

        public void Compose(IUmbracoBuilder builder) {

            // Parse the raw config value since we can't use dependency injection in a composer
            bool disableDefaultPlugin = builder.Config.GetSection("Limbo:Feedback:DisableDefaultPlugin").Value.ToBoolean();

            // Register the configuration
            builder.Services.AddOptions<FeedbackSettings>()
                .Bind(builder.Config.GetSection("Limbo:Feedback"), o => o.BindNonPublicProperties = true)
                .ValidateDataAnnotations();

            // Register services
            builder.Services.AddSingleton<FeedbackPluginDependencies>();
            builder.Services.AddSingleton<FeedbackDatabaseService>();
            builder.Services.AddSingleton<FeedbackService>();

            // Initialize a plugins collection
            builder.FeedbackPlugins();
            if (!disableDefaultPlugin) builder.FeedbackPlugins().Append<DefaultFeedbackPlugin>();

            // Register the content app factory
            builder.ContentApps().Append<FeedbackContentApp>();

            builder.ManifestFilters().Append<FeedbackManifestFilter>();

        }

    }

}