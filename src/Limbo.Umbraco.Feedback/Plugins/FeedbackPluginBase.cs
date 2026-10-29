using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Limbo.Umbraco.Feedback.Events;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Limbo.Umbraco.Feedback.Models.Users;
using Limbo.Umbraco.Feedback.Models.Workspaces;
using Limbo.Umbraco.Feedback.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Feedback.Plugins;

/// <summary>
/// Abstract implementation of the <see cref="IFeedbackPlugin"/> interface.
/// </summary>
public abstract class FeedbackPluginBase : IFeedbackPlugin {

    private readonly FeedbackPluginDependencies _dependencies;

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="dependencies"/>.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    protected FeedbackPluginBase(FeedbackPluginDependencies dependencies) {
        _dependencies = dependencies;
    }

    #endregion

    /// <summary>
    /// Method invoked when a new feedback entry is being added.
    /// </summary>
    /// <param name="args">The event args.</param>
    public virtual void OnEntryAdding(EntryAddingEventArgs args) { }

    /// <summary>
    /// Method invoked when a new feedback entry has been added.
    /// </summary>
    /// <param name="args">The event args.</param>
    public virtual void OnEntryAdded(EntryAddedEventArgs args) { }

    /// <summary>
    /// Method invoked when a feedback entry is being updated.
    /// </summary>
    /// <param name="args">The event args.</param>
    public virtual void OnEntryUpdating(EntryUpdatingEventArgs args) { }

    /// <summary>
    /// Method invoked when a feedback entry has been updated.
    /// </summary>
    /// <param name="args">The event args.</param>
    public virtual void OnEntryUpdated(EntryUpdatedEventArgs args) { }

    /// <summary>
    /// Method invoked when the status of a feedback entry is being updated.
    /// </summary>
    /// <param name="service">A reference to the current feedback service.</param>
    /// <param name="entry">The feedback entry that is being updated.</param>
    /// <param name="newStatus">The new status of the entry.</param>
    /// <returns><c>true</c> if the feedback plugin handled the entry; otherwise, <c>false</c>.</returns>
    public virtual bool OnStatusChanging(FeedbackService service, FeedbackEntry entry, FeedbackStatus newStatus) {
        return true;
    }

    /// <summary>
    /// Method invoked when the status of a feedback entry has been updated.
    /// </summary>
    /// <param name="service">A reference to the current feedback service.</param>
    /// <param name="entry">The feedback entry that was updated.</param>
    /// <param name="oldStatus">The status of the entry prior to the update.</param>
    /// <param name="newStatus">The status of the entry after the update.</param>
    public virtual void OnStatusChanged(FeedbackService service, FeedbackEntry entry, FeedbackStatus oldStatus, FeedbackStatus newStatus) { }

    /// <summary>
    /// Method invoked when the assigned user of a feedback entry is changed.
    /// </summary>
    /// <param name="service">A reference to the current feedback service.</param>
    /// <param name="entry">The feedback entry.</param>
    /// <param name="newUser">A reference to the new user. If the entry is updated to not be assigned to anyone, this value will be <c>null</c>.</param>
    /// <returns><c>true</c> if the feedback plugin handled the entry; otherwise, <c>false</c>.</returns>
    public virtual bool OnUserAssigning(FeedbackService service, FeedbackEntry entry, IFeedbackUser? newUser) {
        return true;
    }

    /// <summary>
    /// Method invoked when the assigned user of a feedback entry has been updated.
    /// </summary>
    /// <param name="service">A reference to the current feedback service.</param>
    /// <param name="entry">The feedback entry.</param>
    /// <param name="oldUser">The assigned user prior the update.</param>
    /// <param name="newUser">The assigned user after the update.</param>
    public virtual void OnUserAssigned(FeedbackService service, FeedbackEntry entry, IFeedbackUser? oldUser, IFeedbackUser? newUser) { }

    /// <summary>
    /// Gets the site with the specified <paramref name="key"/>, or <c>null</c> if not found.
    /// </summary>
    /// <param name="key">The key (GUID) of the site.</param>
    /// <param name="site">When this method returns, holds the information about the site if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if a site was found; otherwise, <c>false</c>.</returns>
    public virtual bool TryGetSite(Guid key, [NotNullWhen(true)] out FeedbackSiteSettings? site) {

        IPublishedContent? content = _dependencies.PublishedContentCache.GetById(key);

        if (content is not null) {
            if (_dependencies.DomainCache.HasAssigned(content.Id, false)) {
                site = new FeedbackSiteSettings(content);
                return true;
            }
        }

        site = null;
        return false;

    }

    /// <summary>
    /// Attempts to get the parent site of the specified <paramref name="content"/>.
    /// </summary>
    /// <param name="content">The content representing a page under the site.</param>
    /// <param name="site">When this method returns, holds an instance of <see cref="FeedbackSiteSettings"/> representing the parent site if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryGetSite(IContent content, [NotNullWhen(true)] out FeedbackSiteSettings? site) {

        IPublishedContent? scope = _dependencies.PublishedContentCache.GetById(content.Key);

        while (scope is not null) {

            if (_dependencies.FeedbackSettings.SiteContentTypes.Contains(scope.ContentType.Alias)) {
                site = new FeedbackSiteSettings(scope);
                return true;
            }

            scope = scope.Parent<IPublishedContent>(_dependencies.DocumentNavigationQueryService, _dependencies.PublishedContentStatusFilteringService);

        }

        site = null;
        return false;

    }

    /// <summary>
    /// Returns the user with the specified <paramref name="id"/>, or <c>null</c> if not found.
    /// </summary>
    /// <param name="id">The numeric ID of the user.</param>
    /// <returns>An instance of <see cref="IFeedbackUser"/> if successful; otherwise, <c>null</c>.</returns>
    public virtual IFeedbackUser? GetUser(int id) {
        return GetUsers().FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Returns the user with the specified <paramref name="key"/>, or <c>null</c> if not found.
    /// </summary>
    /// <param name="key">The GUID key of the user.</param>
    /// <returns>An instance of <see cref="IFeedbackUser"/> if successful; otherwise, <c>null</c>.</returns>
    public virtual IFeedbackUser? GetUser(Guid key) {
        return GetUsers().FirstOrDefault(x => x.Key == key);
    }

    /// <summary>
    /// Gets the user with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The numeric ID of the user.</param>
    /// <param name="user">When this method returns, holds an instance of <see cref="IFeedbackUser"/> if successful; otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if a user was found; otherwise, <c>false</c>.</returns>
    public virtual bool TryGetUser(int id, [NotNullWhen(true)] out IFeedbackUser? user) {
        user = GetUsers().FirstOrDefault(x => x.Id == id);
        return user != null;
    }

    /// <summary>
    /// Gets the user with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key (GUID) of the user.</param>
    /// <param name="user">When this method returns, holds an instance of <see cref="IFeedbackUser"/> if successful; otherwise <c>false</c>.</param>
    /// <returns><c>true</c> if a user was found; otherwise, <c>false</c>.</returns>
    public virtual bool TryGetUser(Guid key, [NotNullWhen(true)] out IFeedbackUser? user) {
        user = GetUsers().FirstOrDefault(x => x.Key == key);
        return user != null;
    }

    /// <summary>
    /// Returns an array of all feedback users.
    /// </summary>
    /// <returns>An array of <see cref="IFeedbackUser"/>.</returns>
    public virtual IReadOnlyList<IFeedbackUser> GetUsers() {
        return _dependencies.UserService
            .GetAll(0, int.MaxValue, out _)
            .Where(x => x.IsApproved)
            .Select(x => (IFeedbackUser) new FeedbackUser(x))
            .ToArray();
    }

    /// <summary>
    /// Virtual method for getting the feedback workspace view for the specified <paramref name="content"/>.
    ///
    /// The view is shown for content items whose content type alias matches one of the configured site content
    /// types, and for content items whose alias matches one of the configured page content types - provided a
    /// parent site can be resolved.
    /// </summary>
    /// <param name="content">The content item being rendered.</param>
    /// <param name="result">The workspace view, or <c>null</c> if the view shouldn't be shown for <paramref name="content"/>.</param>
    /// <returns><c>true</c> if a workspace view was configured; otherwise <c>false</c>.</returns>
    public virtual bool TryGetWorkspaceView(IContent content, [NotNullWhen(true)] out FeedbackWorkspaceView? result) {

        // If the ID is 0 it means that the content node is currently being created, in which case it doesn't
        // really make sense to show the workspace view
        if (content.Id == 0) {
            result = null;
            return false;
        }

        // If the content type alias matches a site type, we show the view with site level information
        if (_dependencies.FeedbackSettings.SiteContentTypes.Contains(content.ContentType.Alias)) {
            result = GetWorkspaceViewForSite(content);
            return result != null;
        }

        // If the content type alias matches a page type, we try to get a reference to the site the page belongs
        // to, and then show the view for the page
        if (_dependencies.FeedbackSettings.PageContentTypes.Contains(content.ContentType.Alias) && TryGetSite(content, out FeedbackSiteSettings? site)) {
            result = GetWorkspaceViewForPage(site, content);
            return result != null;
        }

        result = null;
        return false;

    }

    /// <summary>
    /// Returns the workspace view for the specified <paramref name="site"/>.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <returns>An instance of <see cref="FeedbackWorkspaceView"/>.</returns>
    /// <remarks>Override the method and return <c>null</c> for a given site if the view shouldn't be shown.</remarks>
    protected virtual FeedbackWorkspaceView? GetWorkspaceViewForSite(IContent site) {
        return new FeedbackWorkspaceView(site.Key);
    }

    /// <summary>
    /// Returns the workspace view for the specified <paramref name="page"/>.
    /// </summary>
    /// <param name="site">The site.</param>
    /// <param name="page">The page.</param>
    /// <returns>An instance of <see cref="FeedbackWorkspaceView"/>.</returns>
    /// <remarks>Override the method and return <c>null</c> for a given page if the view shouldn't be shown.</remarks>
    protected virtual FeedbackWorkspaceView? GetWorkspaceViewForPage(FeedbackSiteSettings site, IContent page) {
        return new FeedbackWorkspaceView(site.Key, page.Key);
    }

}
