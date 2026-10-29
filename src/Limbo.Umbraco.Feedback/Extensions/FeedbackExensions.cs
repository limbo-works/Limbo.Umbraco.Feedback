using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Limbo.Umbraco.Feedback.Events;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Models.Workspaces;
using Limbo.Umbraco.Feedback.Plugins;
using Umbraco.Cms.Core.Models;

namespace Limbo.Umbraco.Feedback.Extensions;

/// <summary>
/// Static class with various Feedback related extension methods.
/// </summary>
public static class FeedbackExensions {

    /// <summary>
    /// Gets the site with the specified <paramref name="key"/>, or <c>null</c> if not found.
    ///
    /// The site is found by asking each registered feedback plugin whether they know a site matching
    /// <paramref name="key"/>. The method will return once it's finds the first provider that knows the site.
    /// </summary>
    /// <param name="collection">A collection with the registered feedback plugins.</param>
    /// <param name="key">The key (GUID) of the site.</param>
    /// <param name="site">When this method returns, holds the information about the site if successful; otherwise,
    /// <c>null</c>.</param>
    /// <returns><c>true</c> if a site was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetSite(this FeedbackPluginCollection collection, Guid key, [NotNullWhen(true)] out FeedbackSiteSettings? site) {

        foreach (IFeedbackPlugin plugin in collection) {
            if (plugin.TryGetSite(key, out site)) {
                return true;
            }
        }

        site = null;
        return false;

    }

    /// <summary>
    /// Attempts to get the parent site of the specified <paramref name="content"/>.
    /// </summary>
    /// <param name="collection">A collection with the registered feedback plugins.</param>
    /// <param name="content">The content representing a page under the site.</param>
    /// <param name="site">When this method returns, holds an instance of <see cref="FeedbackSiteSettings"/> representing the parent site if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetSite(this FeedbackPluginCollection collection, IContent content, [NotNullWhen(true)] out FeedbackSiteSettings? site) {

        foreach (IFeedbackPlugin plugin in collection) {
            if (plugin.TryGetSite(content, out site)) {
                return true;
            }
        }

        site = null;
        return false;

    }

    /// <summary>
    /// Gets the feedback workspace view for the specified <paramref name="content"/> item, or <c>false</c> if no
    /// feedback plugins provide a view for <paramref name="content"/>.
    ///
    /// The view is found by asking each registered feedback plugin whether they provide a view for
    /// <paramref name="content"/>. The method will return once it finds the first plugin that returns a view.
    /// </summary>
    /// <param name="collection">A collection with the registered feedback plugins.</param>
    /// <param name="content">The <see cref="IContent"/> to show the workspace view for.</param>
    /// <param name="result">When this method returns, holds the workspace view if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if a workspace view was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetWorkspaceView(this FeedbackPluginCollection collection, IContent content, [NotNullWhen(true)] out FeedbackWorkspaceView? result) {

        foreach (IFeedbackPlugin plugin in collection) {
            if (plugin.TryGetWorkspaceView(content, out result)) {
                return true;
            }
        }

        result = null;
        return false;

    }

    /// <summary>
    /// Sets the specified event <paramref name="args"/> as cancelled.
    /// </summary>
    /// <param name="args">The event args.</param>
    /// <param name="message">The message.</param>
    public static void Cancel(this EntryAddingEventArgs args, string? message) {
        args.Cancel = true;
        args.Message = message;
    }

    /// <summary>
    /// Sets the specified event <paramref name="args"/> as cancelled.
    /// </summary>
    /// <param name="args">The event args.</param>
    /// <param name="message">The message.</param>
    /// <param name="statusCode">The HTTP status code to be used for the response to the user.</param>
    public static void Cancel(this EntryAddingEventArgs args, string? message, HttpStatusCode? statusCode) {
        args.Cancel = true;
        args.Message = message;
        args.StatusCode = statusCode;
    }

}