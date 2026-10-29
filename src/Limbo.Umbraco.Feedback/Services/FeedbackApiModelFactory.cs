using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.Feedback.Models.Api;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Ratings;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Limbo.Umbraco.Feedback.Models.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Limbo.Umbraco.Feedback.Services;

/// <summary>
/// Service for mapping feedback entries to the API models returned by the management API.
/// </summary>
/// <remarks>
/// The mapping used to live in the back office controller. It now sits in a service so the management API
/// controllers can share it. Note that the models no longer contain server side localized labels or relative date
/// strings - since Umbraco 14 the back office is localized client side.
/// </remarks>
public class FeedbackApiModelFactory {

    private readonly FeedbackService _feedbackService;
    private readonly IContentService _contentService;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IUmbracoContextFactory _umbracoContextFactory;

    /// <summary>
    /// Initializes a new instance based on the specified dependencies.
    /// </summary>
    public FeedbackApiModelFactory(FeedbackService feedbackService, IContentService contentService, IPublishedContentCache publishedContentCache, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextFactory umbracoContextFactory) {
        _feedbackService = feedbackService;
        _contentService = contentService;
        _publishedContentCache = publishedContentCache;
        _publishedUrlProvider = publishedUrlProvider;
        _umbracoContextFactory = umbracoContextFactory;
    }

    /// <summary>
    /// Maps the specified <paramref name="list"/> to a <see cref="FeedbackResultApiModel"/>.
    /// </summary>
    /// <param name="site">The site the entries belong to.</param>
    /// <param name="list">The paginated list of entries.</param>
    /// <param name="options">The options the list was fetched with.</param>
    /// <returns>An instance of <see cref="FeedbackResultApiModel"/>.</returns>
    public FeedbackResultApiModel GetResult(FeedbackSiteSettings site, FeedbackEntryList list, FeedbackGetEntriesOptions options) {

        SiteApiModel siteModel = new(site);

        Dictionary<Guid, PageApiModel?> pages = new();

        List<EntryApiModel> entries = new();

        // A single Umbraco context reference is enough for resolving the URLs of every page in the list
        using UmbracoContextReference context = _umbracoContextFactory.EnsureUmbracoContext();

        foreach (FeedbackEntry entry in list.Entries) {
            entries.Add(GetEntry(entry, site, siteModel, pages));
        }

        return new FeedbackResultApiModel {
            Site = siteModel,
            Entries = new EntryListApiModel {
                Pagination = new PaginationApiModel {
                    Page = list.Page,
                    Pages = list.PerPage == 0 ? 0 : (int) Math.Ceiling(list.Total / (double) list.PerPage),
                    Limit = list.PerPage,
                    Total = list.Total,
                    Offset = Math.Max(0, list.Page - 1) * list.PerPage
                },
                Sorting = new SortingApiModel { Field = options.SortField, Order = options.SortOrder },
                Data = entries
            }
        };

    }

    /// <summary>
    /// Maps a single <paramref name="entry"/> to an <see cref="EntryApiModel"/>.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="site">The site the entry belongs to.</param>
    /// <returns>An instance of <see cref="EntryApiModel"/>.</returns>
    public EntryApiModel GetEntry(FeedbackEntry entry, FeedbackSiteSettings site) {
        using UmbracoContextReference context = _umbracoContextFactory.EnsureUmbracoContext();
        return GetEntry(entry, site, new SiteApiModel(site), new Dictionary<Guid, PageApiModel?>());
    }

    private EntryApiModel GetEntry(FeedbackEntry entry, FeedbackSiteSettings site, SiteApiModel siteModel, Dictionary<Guid, PageApiModel?> pages) {

        // Normally the rating and status will always be found, but a site may have dropped one that existing
        // entries still reference - in which case we fall back to a synthetic "not found" value
        if (!site.TryGetRating(entry.Dto.Rating, out FeedbackRating? rating)) {
            rating = new FeedbackRating(entry.Dto.Rating, "notFound");
        }

        if (!site.TryGetStatus(entry.Dto.Status, out FeedbackStatus? status)) {
            status = new FeedbackStatus(entry.Dto.Status, "notFound");
        }

        if (!pages.TryGetValue(entry.PageKey, out PageApiModel? page)) {
            pages[entry.PageKey] = page = TryGetPage(entry.PageKey, out PageApiModel? p) ? p : null;
        }

        IFeedbackUser? user = null;
        if (entry.Dto.AssignedTo != Guid.Empty) {
            _feedbackService.TryGetUser(entry.Dto.AssignedTo, out user);
        }

        return new EntryApiModel(entry, siteModel, page, new StatusApiModel(status), new RatingApiModel(rating), user);

    }

    private bool TryGetPage(Guid key, [NotNullWhen(true)] out PageApiModel? result) {

        IPublishedContent? publishedContent = _publishedContentCache.GetById(key);
        if (publishedContent != null) {
            result = new PageApiModel(publishedContent, _publishedUrlProvider.GetUrl(publishedContent, UrlMode.Absolute));
            return true;
        }

        IContent? content = _contentService.GetById(key);
        if (content != null) {
            result = new PageApiModel(content);
            return true;
        }

        result = null;
        return false;

    }

}
