using Limbo.Umbraco.Feedback.ContentApps;
using Limbo.Umbraco.Feedback.Extensions;
using Limbo.Umbraco.Feedback.Manifests;
using Limbo.Umbraco.Feedback.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Limbo.Umbraco.Feedback.Composers {

#pragma warning disable 1591

    public class FeedbackComposer : IComposer {

        public void Compose(IUmbracoBuilder builder) {

            // Register services
            builder.Services.AddScoped<FeedbackDatabaseService>();
            builder.Services.AddScoped<FeedbackService>();

            // Initialize a plugins collection
            builder.FeedbackPlugins();

            // Register the content app factory
            builder.ContentApps().Append<FeedbackContentApp>();

            builder.ManifestFilters().Append<FeedbackManifestFilter>();

        }

    }

}