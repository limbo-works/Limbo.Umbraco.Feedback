using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Limbo.Umbraco.Feedback.Plugins {

    /// <summary>
    /// Class wrapping the dependencies for <see cref="FeedbackPluginBase"/> and related classes.
    /// </summary>
    public class FeedbackPluginDependencies {

        /// <summary>
        /// Gets a reference to the current <see cref="IDomainService"/>.
        /// </summary>
        public IDomainService DomainService { get; }

        /// <summary>
        /// Gets a reference to the current <see cref="UmbracoContextAccessor"/>.
        /// </summary>
        public IUmbracoContextAccessor UmbracoContextAccessor { get; }

        /// <summary>
        /// Gets a reference to the current <see cref="IUmbracoContext"/> if available.
        /// </summary>
        public IUmbracoContext? UmbracoContext => UmbracoContextAccessor
            .TryGetUmbracoContext(out IUmbracoContext? umbracoContext) ? umbracoContext : null;

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="domainService"/> and <paramref name="umbracoContextAccessor"/>.
        /// </summary>
        /// <param name="domainService">The current domain service.</param>
        /// <param name="umbracoContextAccessor">The current Umbraco context accessor.</param>
        public FeedbackPluginDependencies(IDomainService domainService, IUmbracoContextAccessor umbracoContextAccessor) {
            DomainService = domainService;
            UmbracoContextAccessor = umbracoContextAccessor;
        }

    }

}