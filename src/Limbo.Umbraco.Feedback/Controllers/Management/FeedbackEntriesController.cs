using System;
using Asp.Versioning;
using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Models.Api;
using Limbo.Umbraco.Feedback.Models.Api.Post;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Limbo.Umbraco.Feedback.Models.Users;
using Limbo.Umbraco.Feedback.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skybrud.Essentials.Enums;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Management;

/// <summary>
/// Endpoints for listing and maintaining feedback entries from the back office.
/// </summary>
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = FeedbackConstants.ApiName)]
public class FeedbackEntriesController : FeedbackManagementControllerBase {

    private const int DefaultPerPage = 10;

    private readonly FeedbackService _feedbackService;
    private readonly FeedbackApiModelFactory _factory;
    private readonly IContentService _contentService;
    private readonly IUserService _userService;

    #region Constructors

    public FeedbackEntriesController(FeedbackService feedbackService, FeedbackApiModelFactory factory, IContentService contentService, IUserService userService) {
        _feedbackService = feedbackService;
        _factory = factory;
        _contentService = contentService;
        _userService = userService;
    }

    #endregion

    #region Reading

    /// <summary>
    /// Returns a paginated list of the feedback entries of the site with the specified <paramref name="key"/>.
    /// </summary>
    [HttpGet("entries/site/{key:guid}")]
    [ProducesResponseType<FeedbackResultApiModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetEntriesForSite(Guid key, int page = 1, string? sort = null, string? order = null, string? rating = null, string? responsible = null, string? status = null, string? type = null) {

        if (!_feedbackService.TryGetSite(key, out FeedbackSiteSettings? site)) return NotFound("Site not found.");

        FeedbackGetEntriesOptions options = GetOptions(page, sort, order, rating, responsible, status, type);
        options.SiteKey = key;

        return Ok(_factory.GetResult(site, _feedbackService.GetEntries(options), options));

    }

    /// <summary>
    /// Returns a paginated list of the feedback entries of the page with the specified <paramref name="key"/>.
    /// </summary>
    [HttpGet("entries/page/{key:guid}")]
    [ProducesResponseType<FeedbackResultApiModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetEntriesForPage(Guid key, int page = 1, string? sort = null, string? order = null, string? rating = null, string? responsible = null, string? status = null, string? type = null) {

        IContent? content = _contentService.GetById(key);
        if (content is null) return NotFound("Page not found.");

        if (!_feedbackService.TryGetSite(content, out FeedbackSiteSettings? site)) return NotFound("Site not found.");

        FeedbackGetEntriesOptions options = GetOptions(page, sort, order, rating, responsible, status, type);
        options.PageKey = content.Key;

        return Ok(_factory.GetResult(site, _feedbackService.GetEntries(options), options));

    }

    #endregion

    #region Writing

    /// <summary>
    /// Sets the status of the entry with the specified <paramref name="key"/>.
    /// </summary>
    [HttpPost("entries/{key:guid}/status")]
    [ProducesResponseType<EntryApiModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult SetStatus(Guid key, [FromBody] SetStatusModel model) {

        if (model.Status == Guid.Empty) return BadRequest("A status key must be specified.");

        if (!TryGetEntryAndSite(key, out FeedbackEntry? entry, out FeedbackSiteSettings? site, out IActionResult? error)) return error;

        if (!site.TryGetStatus(model.Status, out FeedbackStatus? status)) {
            return BadRequest("A status with the specified key does not exist for the site of the entry.");
        }

        _feedbackService.SetStatus(entry, status);

        return Ok(_factory.GetEntry(entry, site));

    }

    /// <summary>
    /// Sets the user responsible for the entry with the specified <paramref name="key"/>.
    /// </summary>
    [HttpPost("entries/{key:guid}/responsible")]
    [ProducesResponseType<EntryApiModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult SetResponsible(Guid key, [FromBody] SetResponsibleModel model) {

        if (!TryGetEntryAndSite(key, out FeedbackEntry? entry, out FeedbackSiteSettings? site, out IActionResult? error)) return error;

        if (model.Responsible == Guid.Empty) {
            _feedbackService.SetAssignedTo(entry, null);
        } else {
            if (!_feedbackService.TryGetUser(model.Responsible, out IFeedbackUser? user)) {
                return BadRequest("A user with the specified key could not be found.");
            }
            _feedbackService.SetAssignedTo(entry, user);
        }

        return Ok(_factory.GetEntry(entry, site));

    }

    /// <summary>
    /// Archives the entry with the specified <paramref name="key"/>.
    /// </summary>
    /// <remarks>Was a <c>GET</c> endpoint prior to Umbraco 17.</remarks>
    [HttpPost("entries/{key:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Archive(Guid key) {

        FeedbackEntry? entry = _feedbackService.GetEntryByKey(key);
        if (entry is null) return NotFound();

        _feedbackService.Archive(entry);

        return NoContent();

    }

    /// <summary>
    /// Deletes the entry with the specified <paramref name="key"/>.
    /// </summary>
    /// <remarks>Was a <c>GET</c> endpoint prior to Umbraco 17.</remarks>
    [HttpDelete("entries/{key:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid key) {

        FeedbackEntry? entry = _feedbackService.GetEntryByKey(key);
        if (entry is null) return NotFound();

        _feedbackService.Delete(entry);

        return NoContent();

    }

    #endregion

    #region Private helper methods

    private bool TryGetEntryAndSite(Guid key, out FeedbackEntry entry, out FeedbackSiteSettings site, out IActionResult error) {

        entry = null!;
        site = null!;
        error = null!;

        FeedbackEntry? e = _feedbackService.GetEntryByKey(key);
        if (e is null) {
            error = NotFound("An entry with the specified key could not be found.");
            return false;
        }

        if (!_feedbackService.TryGetSite(e.SiteKey, out FeedbackSiteSettings? s)) {
            error = NotFound("The site of the entry could not be found.");
            return false;
        }

        entry = e;
        site = s;
        return true;

    }

    private FeedbackGetEntriesOptions GetOptions(int page, string? sort, string? order, string? rating, string? responsible, string? status, string? type) {

        FeedbackGetEntriesOptions options = new() {
            Page = Math.Max(1, page),
            PerPage = DefaultPerPage
        };

        switch (sort) {

            case "rating":
                options.SortField = EntriesSortField.Rating;
                options.SortOrder = EnumUtils.ParseEnum(order, EntriesSortOrder.Asc);
                break;

            case "status":
                options.SortField = EntriesSortField.Status;
                options.SortOrder = EnumUtils.ParseEnum(order, EntriesSortOrder.Asc);
                break;

            default:
                options.SortField = EntriesSortField.CreateDate;
                options.SortOrder = EnumUtils.ParseEnum(order, EntriesSortOrder.Desc);
                break;

        }

        if (Guid.TryParse(rating, out Guid ratingKey)) {
            options.Rating = ratingKey;
        }

        // The numeric variant is kept for backwards compatibility - the back office now always sends a key
        if (int.TryParse(responsible, out int responsibleId)) {
            options.Responsible = _userService.GetUserById(responsibleId)?.Key;
        } else if (Guid.TryParse(responsible, out Guid responsibleKey)) {
            options.Responsible = responsibleKey;
        }

        if (Guid.TryParse(status, out Guid statusKey)) {
            options.Status = statusKey;
        }

        options.Type = EnumUtils.ParseEnum(type, FeedbackEntryType.All);

        return options;

    }

    #endregion

}
