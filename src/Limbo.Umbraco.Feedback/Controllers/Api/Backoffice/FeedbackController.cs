using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Limbo.Umbraco.Feedback.Models.Api;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Ratings;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Limbo.Umbraco.Feedback.Models.Users;
using Limbo.Umbraco.Feedback.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Enums;
using Skybrud.Essentials.Json.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Api.Backoffice {

    [PluginController("Limbo")]
    public class FeedbackAdminController : UmbracoAuthorizedApiController {

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly FeedbackService _feedbackService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IContentService _contentService;
        private readonly IUserService _userService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        #region Constructors

        public FeedbackAdminController(IUmbracoContextAccessor umbracoContextAccessor, FeedbackService feedbackService, ILocalizedTextService localizedTextService, IContentService contentService, IUserService userService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor) {
            _umbracoContextAccessor = umbracoContextAccessor;
            _feedbackService = feedbackService;
            _localizedTextService = localizedTextService;
            _contentService = contentService;
            _userService = userService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        #endregion

        #region Public API methods

        [HttpGet]
        public object Archive(Guid key) {

            var entry = _feedbackService.GetEntryByKey(key);
            if (entry == null) {
                return NotFound();
            }

            _feedbackService.Archive(entry);

            return entry;

        }

        [HttpGet]
        public object Delete(Guid key) {

            var entry = _feedbackService.GetEntryByKey(key);
            if (entry == null) {
                return NotFound();
            }

            _feedbackService.Delete(entry);

            return entry;

        }

        public object GetEntriesForSite(Guid key, int page = 1, string? sort = null, string? order = null, string? rating = null, string? responsible = null, string? status = null, string? type = null) {

            CultureInfo culture = new(_backOfficeSecurityAccessor.BackOfficeSecurity!.CurrentUser!.Language!);

            if (_feedbackService.TryGetSite(key, out FeedbackSiteSettings? site) == false) {
                return NotFound();
            }

            FeedbackGetEntriesOptions options = new() {
                Page = page,
                PerPage = 10,
                SiteKey = key
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

            if (int.TryParse(responsible, out int responsibleId)) {
                options.Responsible = _userService.GetUserById(responsibleId)?.Key;
            } else if (Guid.TryParse(responsible, out Guid responsibleKey)) {
                options.Responsible = responsibleKey;
            }

            if (Guid.TryParse(status, out Guid statusKey)) {
                options.Status = statusKey;
            }

            options.Type = EnumUtils.ParseEnum(type, FeedbackEntryType.All);












            var result = _feedbackService.GetEntries(options);

            var siteModel = new SiteApiModel(site, _localizedTextService, culture);

            List<EntryApiModel> entries = new();

            Dictionary<Guid, PageApiModel> pages = new();

            foreach (var entry in result.Entries) {

                if (!site.TryGetRating(entry.Dto.Rating, out var er)) {
                    er = new FeedbackRating(entry.Dto.Rating, "not-found");
                }

                if (!site.TryGetStatus(entry.Dto.Status, out var es)) {
                    es = new FeedbackStatus(entry.Dto.Status, "not-found");
                }

                if (!pages.TryGetValue(entry.PageKey, out PageApiModel? pageModel) && _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext)) {
                    var c1 = umbracoContext.Content?.GetById(entry.PageKey);
                    if (c1 != null) {
                        pages.Add(entry.PageKey, pageModel = new PageApiModel(c1));
                    } else {
                        var c2 = _contentService.GetById(entry.PageKey);
                        if (c2 != null) {
                            pages.Add(entry.PageKey, pageModel = new PageApiModel(c2));

                        }
                    }
                }

                IFeedbackUser? user = null;
                if (entry.Dto.AssignedTo != Guid.Empty) {
                    _feedbackService.TryGetUser(entry.Dto.AssignedTo, out user);
                }

                var r = new RatingApiModel(er, _localizedTextService, culture);
                var s = new StatusApiModel(es, _localizedTextService, culture);

                var em = new EntryApiModel(entry, siteModel, pageModel, s, r, user);

                em.CreateDateDiff = GetDiff(em.CreateDate, culture);
                em.UpdateDateDiff = GetDiff(em.UpdateDate, culture);

                entries.Add(em);

            }

            return new {
                site = siteModel,
                entries = new {
                    pagination = new {
                        page,
                        pages = (int) Math.Ceiling(result.Total / (double) result.PerPage),
                        limit = result.PerPage,
                        total = result.Total,
                        offset = (result.Page - 1) * result.PerPage,
                    },
                    sorting = new { field = options.SortField, order = options.SortOrder },
                    data = entries
                }
            };

        }

        public object GetEntriesForPage(Guid key, int page = 1, string? sort = null, string? order = null, string? rating = null, string? responsible = null, string? status = null, string? type = null) {

            CultureInfo culture = new(_backOfficeSecurityAccessor.BackOfficeSecurity!.CurrentUser!.Language!);

            // Get a reference to the current page
            IContent? content = _contentService.GetById(key);
            if (content is null) return NotFound("Page not found.");

            // Attempt to find the parent site
            if (!_feedbackService.TryGetSite(content, out FeedbackSiteSettings? site)) return NotFound("Site not found.");

            FeedbackGetEntriesOptions options = new() {
                Page = page,
                PerPage = 10,
                PageKey = content.Key
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

            if (int.TryParse(responsible, out int responsibleId)) {
                options.Responsible = _userService.GetUserById(responsibleId)?.Key;
            } else if (Guid.TryParse(responsible, out Guid responsibleKey)) {
                options.Responsible = responsibleKey;
            }

            if (Guid.TryParse(status, out Guid statusKey)) {
                options.Status = statusKey;
            }

            options.Type = EnumUtils.ParseEnum(type, FeedbackEntryType.All);

            var result = _feedbackService.GetEntries(options);

            var siteModel = new SiteApiModel(site, _localizedTextService, culture);

            List<EntryApiModel> entries = new();

            foreach (var entry in result.Entries) {

                IFeedbackUser? user = null;
                if (entry.Dto.AssignedTo != Guid.Empty) {
                    _feedbackService.TryGetUser(entry.Dto.AssignedTo, out user);
                }

                entries.Add(MapEntry(entry, site, user, culture));

            }

            return new {
                site = siteModel,
                entries = new {
                    pagination = new {
                        page,
                        pages = (int) Math.Ceiling(result.Total / (double) result.PerPage),
                        limit = result.PerPage,
                        total = result.Total,
                        offset = (result.Page - 1) * result.PerPage,
                    },
                    sorting = new { field = options.SortField, order = options.SortOrder },
                    data = entries
                }
            };

        }

        [HttpGet]
        public object GetUsers() {
            return _feedbackService.GetUsers();
        }

        [HttpPost]
        public object SetStatus([FromBody] JObject model) {

            CultureInfo culture = new(_backOfficeSecurityAccessor.BackOfficeSecurity!.CurrentUser!.Language!);

            Guid entryKey = model.GetGuid("entry");
            Guid statusKey = model.GetGuid("status");

            if (entryKey == Guid.Empty) {
                return BadRequest();
            }

            if (statusKey == Guid.Empty) {
                return BadRequest();
            }

            // Get the entry
            FeedbackEntry? entry = _feedbackService.GetEntryByKey(entryKey);
            if (entry == null) {
                return NotFound();
            }

            // Get the site of the entry
            if (_feedbackService.TryGetSite(entry.SiteKey, out FeedbackSiteSettings? site) == false) {
                throw new Exception();
            }

            // Get the status
            if (site.TryGetStatus(statusKey, out FeedbackStatus? status) == false) {
                throw new Exception();
            }

            _feedbackService.SetStatus(entry, status);

            IFeedbackUser? user = null;
            if (entry.Dto.AssignedTo != Guid.Empty) {
                _feedbackService.TryGetUser(entry.Dto.AssignedTo, out user);
            }

            return MapEntry(entry, site, user, culture);

        }

        [HttpPost]
        public object SetResponsible([FromBody] JObject model) {

            CultureInfo culture = new(_backOfficeSecurityAccessor.BackOfficeSecurity!.CurrentUser!.Language!);

            Guid entryKey = model.GetGuid("entry");
            Guid responsibleKey = model.GetGuid("responsible");

            if (entryKey == Guid.Empty) {
                return BadRequest();
            }

            // Get the entry
            FeedbackEntry? entry = _feedbackService.GetEntryByKey(entryKey);
            if (entry == null) {
                return NotFound();
            }

            // Get the site of the entry
            if (_feedbackService.TryGetSite(entry.SiteKey, out FeedbackSiteSettings? site) == false) {
                throw new Exception();
            }

            IFeedbackUser? user = null;
            if (responsibleKey == Guid.Empty) {
                _feedbackService.SetAssignedTo(entry, null);
            } else {
                if (_feedbackService.TryGetUser(responsibleKey, out user) == false) {
                    throw new Exception();
                }
                _feedbackService.SetAssignedTo(entry, user);
            }

            return MapEntry(entry, site, user, culture);

        }

        #endregion

        #region Private helper methods

        private EntryApiModel MapEntry(FeedbackEntry entry, FeedbackSiteSettings site, IFeedbackUser? user, CultureInfo culture) {

            if (!site.TryGetRating(entry.Dto.Rating, out FeedbackRating? rating)) {
                rating = new FeedbackRating(entry.Dto.Rating, "not-found");
            }

            if (!site.TryGetStatus(entry.Dto.Status, out FeedbackStatus? status)) {
                status = new FeedbackStatus(entry.Dto.Status, "not-found");
            }

            var r = new RatingApiModel(rating, _localizedTextService, culture);
            var s = new StatusApiModel(status, _localizedTextService, culture);

            var siteModel = new SiteApiModel(site, _localizedTextService, culture);

            var em =  new EntryApiModel(entry, siteModel, TryGetPage(entry.PageKey, out var page) ? page : null, s, r, user);

            em.CreateDateDiff = GetDiff(em.CreateDate, culture);
            em.UpdateDateDiff = GetDiff(em.UpdateDate, culture);

            return em;

        }

        private bool TryGetPage(Guid key, [NotNullWhen(true)] out PageApiModel? result) {

            _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
            IPublishedContent? publishedContent = umbracoContext?.Content?.GetById(key);
            if (publishedContent != null) {
                result = new PageApiModel(publishedContent);
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

        private string GetDiff(DateTime date, CultureInfo culture) {

            TimeSpan diff = DateTime.UtcNow.Subtract(date);

            int totalSeconds = (int) diff.TotalSeconds;

            return totalSeconds switch {
                >= 60 * 60 * 24 * 2 => Localize("x_days_ago", culture, new Dictionary<string, string?> { { "days", (totalSeconds / 24 / 60 / 60).ToString("N0") } }),
                >= 60 * 60 * 24 => Localize("a_day_ago", culture),
                >= 60 * 60 * 2 => Localize("x_hours_ago", culture, new Dictionary<string, string?> { { "hours", (totalSeconds / 60 / 60).ToString("N0") } }),
                >= 60 * 60 => Localize("an_hour_ago", culture),
                >= 60 * 2 => Localize("x_minutes_ago", culture, new Dictionary<string, string?> { { "minutes", (totalSeconds / 60).ToString("N0") } }),
                >= 60 => Localize("a_minute_ago", culture),
                >= 5 => Localize("x_seconds_ago", culture, new Dictionary<string, string?> { { "seconds", totalSeconds.ToString("N0") } }),
                _ => Localize("now", culture)
            };
        }

        private string Localize(string alias, CultureInfo culture) {

            return _localizedTextService.Localize("feedback", alias, culture);

        }

        private string Localize(string alias, CultureInfo culture, Dictionary<string, string?> tokens) {

            return _localizedTextService.Localize("feedback", alias, culture, tokens);

        }

        #endregion

    }

}