using Limbo.Umbraco.Feedback.Components;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Composers {

    public class MigrationComposer : IComposer {

        public void Compose(IUmbracoBuilder builder) {
            builder.Components().Append<MigrationComponent>();
        }

    }

}