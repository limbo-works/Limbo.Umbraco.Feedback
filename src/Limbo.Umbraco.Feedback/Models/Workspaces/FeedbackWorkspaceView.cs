using System;
using System.Text.Json.Serialization;

namespace Limbo.Umbraco.Feedback.Models.Workspaces;

/// <summary>
/// Class describing the feedback workspace view for a given content item.
///
/// This is the Umbraco 17 replacement for the content app that earlier versions of this package returned from
/// <c>IFeedbackPlugin.TryGetContentApp</c>. Content apps were removed in Umbraco 14 in favour of workspace views,
/// which are registered client side. The server therefore no longer describes the view itself - it only tells the
/// back office <em>whether</em> the view applies to a content item, and which site (and optionally page) it should
/// show feedback for.
/// </summary>
public class FeedbackWorkspaceView {

    /// <summary>
    /// Gets the key (GUID) of the site the feedback should be shown for.
    /// </summary>
    [JsonPropertyName("siteKey")]
    public Guid SiteKey { get; }

    /// <summary>
    /// Gets the key (GUID) of the page the feedback should be limited to, or <see langword="null"/> if the view
    /// should show the feedback of the entire site.
    /// </summary>
    [JsonPropertyName("pageKey")]
    public Guid? PageKey { get; }

    /// <summary>
    /// Gets the icon of the workspace view.
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; }

    /// <summary>
    /// Initializes a new site level workspace view.
    /// </summary>
    /// <param name="siteKey">The key (GUID) of the site.</param>
    public FeedbackWorkspaceView(Guid siteKey) : this(siteKey, null) { }

    /// <summary>
    /// Initializes a new workspace view.
    /// </summary>
    /// <param name="siteKey">The key (GUID) of the site.</param>
    /// <param name="pageKey">The key (GUID) of the page, or <see langword="null"/> for a site level view.</param>
    /// <param name="icon">The icon of the workspace view.</param>
    public FeedbackWorkspaceView(Guid siteKey, Guid? pageKey, string icon = "icon-chat") {
        SiteKey = siteKey;
        PageKey = pageKey;
        Icon = icon;
    }

}
