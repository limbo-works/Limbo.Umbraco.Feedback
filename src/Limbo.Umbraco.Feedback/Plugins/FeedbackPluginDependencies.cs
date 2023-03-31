using Limbo.Umbraco.Feedback.Config;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Limbo.Umbraco.Feedback.Plugins {

    /// <summary>
    /// Class wrapping the dependencies for <see cref="FeedbackPluginBase"/> and related classes.
    /// </summary>
    public class FeedbackPluginDependencies {

        private readonly IOptions<FeedbackSettings> _feedbackSettings;

        /// <summary>
        /// Gets a reference to the current <see cref="IContentService"/>.
        /// </summary>
        public IContentService ContentService { get; }

        /// <summary>
        /// Gets a reference to the current <see cref="IDomainService"/>.
        /// </summary>
        public IDomainService DomainService { get; }

        /// <summary>
        /// Gets a reference to the current <see cref="IUserService"/>.
        /// </summary>
        public IUserService UserService { get; }

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
        /// Gets a reference to the current <see cref="FeedbackSettings"/>.
        /// </summary>
        public FeedbackSettings FeedbackSettings => _feedbackSettings.Value;

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="domainService"/> and <paramref name="umbracoContextAccessor"/>.
        /// </summary>
        /// <param name="contentService">The current content service.</param>
        /// <param name="domainService">The current domain service.</param>
        /// <param name="userService">The current user service.</param>
        /// <param name="umbracoContextAccessor">The current Umbraco context accessor.</param>
        /// <param name="feedbackSettings">The current feedback settings.</param>
        public FeedbackPluginDependencies(IContentService contentService, IDomainService domainService, IUserService userService, IUmbracoContextAccessor umbracoContextAccessor, IOptions<FeedbackSettings> feedbackSettings) {
            _feedbackSettings = feedbackSettings;
            ContentService = contentService;
            DomainService = domainService;
            UserService = userService;
            UmbracoContextAccessor = umbracoContextAccessor;
        }

    }

}