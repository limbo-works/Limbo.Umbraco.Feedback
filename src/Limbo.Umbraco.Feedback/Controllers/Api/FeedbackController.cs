using System;
using System.Net;
using Limbo.Umbraco.Feedback.Extensions;
using Limbo.Umbraco.Feedback.Models.Api.Post;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Ratings;
using Limbo.Umbraco.Feedback.Models.Results;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Plugins;
using Limbo.Umbraco.Feedback.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Api;

/// <summary>
/// Public API for submitting and updating feedback entries.
/// </summary>
/// <remarks>
/// <c>UmbracoApiController</c> was obsoleted in Umbraco 15 and removed in later versions, along with the
/// convention based routing that supported it. The controller is now a plain ASP.NET Core controller with an
/// explicit route - the two endpoints keep the exact same URLs as before (<c>/api/feedback</c> and
/// <c>/api/feedback/{key}</c>), so front-end implementations do not need to change.
/// </remarks>
[ApiController]
[Route("api/feedback")]
public class FeedbackController : Controller {

    private readonly FeedbackService _feedbackService;

    private readonly FeedbackPluginCollection _feedbackPluginCollection;
    private readonly IPublishedContentCache _publishedContentCache;

    #region Constructors

    public FeedbackController(FeedbackService feedbackService, FeedbackPluginCollection feedbackPluginCollection, IPublishedContentCache publishedContentCache) {
        _feedbackService = feedbackService;
        _feedbackPluginCollection = feedbackPluginCollection;
        _publishedContentCache = publishedContentCache;
    }

    #endregion

    #region Public API methods

    [HttpPost("")]
    public IActionResult Add([FromBody] AddCommentModel model) {

        // Get the site
        if (!_feedbackPluginCollection.TryGetSite(model.SiteKey, out FeedbackSiteSettings? site)) {
            return NotFound("A site with the specified key could not be found.");
        }

        // Get the page
        IPublishedContent? page = _publishedContentCache.GetById(model.PageKey);
        if (page == null) {
            return NotFound("A page with the specified key could not be found.");
        }

        // Get the rating
        if (!site.TryGetRating(model.Rating, out FeedbackRating? rating)) {
            return BadRequest("A rating with the specified name does not exist.");
        }

        // Attempt to add the comment
        AddEntryResult result = _feedbackService.AddEntry(site, page, rating, model.Name, model.Email, model.Comment);

        // Return a response matching the result
        return result.Status switch {
            AddEntryStatus.Success => Json(new { key = result.Entry!.Key }, result.StatusCode),
            _ => Json(result)
        };

    }

    [HttpPost("{key:guid}")]
    public IActionResult Update(Guid key, [FromBody] UpdateEntryModel model) {

        // Get the site
        if (!_feedbackPluginCollection.TryGetSite(model.SiteKey, out _)) {
            return NotFound("A site with the specified key could not be found.");
        }

        // Get the page
        IPublishedContent? page = _publishedContentCache.GetById(model.PageKey);
        if (page == null) {
            return NotFound("A page with the specified key could not be found.");
        }

        // Get a reference to the entry
        FeedbackEntry? entry = _feedbackService.GetEntryByKey(key);
        if (entry == null) return NotFound("An entry with the specified key could not be found.");

        // TODO: Should we validate the entry against the specified site and page?

        // Update the properties
        entry.Name = model.Name;
        entry.Email = model.Email;
        entry.Comment = model.Comment;

        // Attempt to add the comment
        UpdateEntryResult result = _feedbackService.UpdateEntry(entry);

        // Return a response matching the result
        return result.Status switch {
            UpdateEntryStatus.Success => Json(new { key = result.Entry!.Key }, result.StatusCode),
            _ => Json(result)
        };

    }

    private static JsonResult Json(object data, HttpStatusCode statusCode) {
        return new JsonResult(data) { StatusCode = (int) statusCode };
    }

    private static JsonResult Json(AddEntryResult result) {
        return Json(new { message = result.Message }, result.StatusCode);
    }

    private static JsonResult Json(UpdateEntryResult result) {
        return Json(new { message = result.Message }, result.StatusCode);
    }

    #endregion

}
