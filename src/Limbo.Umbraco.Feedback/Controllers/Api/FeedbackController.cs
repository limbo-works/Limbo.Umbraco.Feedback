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
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Api {

    public class FeedbackController : UmbracoApiController {

        private readonly FeedbackService _feedbackService;

        private readonly FeedbackPluginCollection _feedbackPluginCollection;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        #region Constructors

        public FeedbackController(FeedbackService feedbackService, FeedbackPluginCollection feedbackPluginCollection, IUmbracoContextAccessor umbracoContextAccessor) {
            _feedbackService = feedbackService;
            _feedbackPluginCollection = feedbackPluginCollection;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        #endregion

        #region Public API methods

        [HttpPost]
        [Route("api/feedback")]
        public object Add([FromBody] AddCommentModel model) {

            // Get site site
            if (!_feedbackPluginCollection.TryGetSite(model.SiteKey, out FeedbackSiteSettings? site)) {
                return NotFound("A site with the specified key could not be found.");
            }

            // Get the page
            _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
            IPublishedContent? page = umbracoContext?.Content?.GetById(model.PageKey);
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

        [HttpPost]
        [Route("api/feedback/{key}")]
        public object Update(Guid key, [FromBody] UpdateEntryModel model) {

            // Get site site
            if (!_feedbackPluginCollection.TryGetSite(model.SiteKey, out _)) {
                return NotFound("A site with the specified key could not be found.");
            }

            // Get the page
            _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
            IPublishedContent? page = umbracoContext?.Content?.GetById(model.PageKey);
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

}