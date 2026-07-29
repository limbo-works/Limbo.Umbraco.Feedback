using Limbo.Umbraco.Feedback.Config;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Limbo.Umbraco.Feedback.Plugins;

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
    /// Gets a reference to the current <see cref="IPublishedContentCache"/>.
    /// </summary>
    /// <remarks>
    /// Replaces the <c>IUmbracoContextAccessor</c> based lookup used prior to Umbraco 17. The published content
    /// cache can be resolved directly, and unlike the Umbraco context it is also available during back office
    /// (management API) requests.
    /// </remarks>
    public IPublishedContentCache PublishedContentCache { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="IDomainCache"/>.
    /// </summary>
    /// <remarks>Used instead of the now obsolete <c>IDomainService.GetAssignedDomains</c>.</remarks>
    public IDomainCache DomainCache { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="IDocumentNavigationQueryService"/>.
    /// </summary>
    /// <remarks>Needed for traversing published content, as <c>IPublishedContent.Parent</c> is obsolete.</remarks>
    public IDocumentNavigationQueryService DocumentNavigationQueryService { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="IPublishedContentStatusFilteringService"/>.
    /// </summary>
    public IPublishedContentStatusFilteringService PublishedContentStatusFilteringService { get; }

    /// <summary>
    /// Gets a reference to the current <see cref="FeedbackSettings"/>.
    /// </summary>
    public FeedbackSettings FeedbackSettings => _feedbackSettings.Value;

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="domainService"/> and <paramref name="publishedContentCache"/>.
    /// </summary>
    /// <param name="contentService">The current content service.</param>
    /// <param name="domainService">The current domain service.</param>
    /// <param name="userService">The current user service.</param>
    /// <param name="publishedContentCache">The current published content cache.</param>
    /// <param name="domainCache">The current domain cache.</param>
    /// <param name="documentNavigationQueryService">The current document navigation query service.</param>
    /// <param name="publishedContentStatusFilteringService">The current published content status filtering service.</param>
    /// <param name="feedbackSettings">The current feedback settings.</param>
    public FeedbackPluginDependencies(IContentService contentService, IDomainService domainService, IUserService userService, IPublishedContentCache publishedContentCache, IDomainCache domainCache, IDocumentNavigationQueryService documentNavigationQueryService, IPublishedContentStatusFilteringService publishedContentStatusFilteringService, IOptions<FeedbackSettings> feedbackSettings) {
        _feedbackSettings = feedbackSettings;
        ContentService = contentService;
        DomainService = domainService;
        UserService = userService;
        PublishedContentCache = publishedContentCache;
        DomainCache = domainCache;
        DocumentNavigationQueryService = documentNavigationQueryService;
        PublishedContentStatusFilteringService = publishedContentStatusFilteringService;
    }

}