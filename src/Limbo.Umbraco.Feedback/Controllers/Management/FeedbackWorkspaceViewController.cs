using System;
using Asp.Versioning;
using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Extensions;
using Limbo.Umbraco.Feedback.Models.Workspaces;
using Limbo.Umbraco.Feedback.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Management;

/// <summary>
/// Endpoint used by the back office to determine whether the feedback workspace view applies to a document.
/// </summary>
/// <remarks>
/// In Umbraco 13 the server decided this by returning a <c>ContentApp</c> from <c>IFeedbackPlugin</c>. Content
/// apps no longer exist, so the decision is exposed as an endpoint instead - the client registers a custom
/// condition that calls it, and only then shows the workspace view.
/// </remarks>
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = FeedbackConstants.ApiName)]
public class FeedbackWorkspaceViewController : FeedbackManagementControllerBase {

    private readonly FeedbackPluginCollection _plugins;
    private readonly IContentService _contentService;

    public FeedbackWorkspaceViewController(FeedbackPluginCollection plugins, IContentService contentService) {
        _plugins = plugins;
        _contentService = contentService;
    }

    /// <summary>
    /// Returns the feedback workspace view for the document with the specified <paramref name="key"/>, or a
    /// <see cref="StatusCodes.Status404NotFound"/> response if the view shouldn't be shown for the document.
    /// </summary>
    /// <param name="key">The key (GUID) of the document.</param>
    [HttpGet("workspace-view/{key:guid}")]
    [ProducesResponseType<FeedbackWorkspaceView>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetWorkspaceView(Guid key) {

        IContent? content = _contentService.GetById(key);
        if (content is null) return NotFound();

        return _plugins.TryGetWorkspaceView(content, out FeedbackWorkspaceView? view) ? Ok(view) : NotFound();

    }

}
