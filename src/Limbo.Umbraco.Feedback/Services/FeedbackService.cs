﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Limbo.Umbraco.Feedback.Exceptions;
using Limbo.Umbraco.Feedback.Extensions;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Ratings;
using Limbo.Umbraco.Feedback.Models.Results;
using Limbo.Umbraco.Feedback.Models.Sites;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Limbo.Umbraco.Feedback.Models.Users;
using Limbo.Umbraco.Feedback.Plugins;
using Microsoft.Extensions.Logging;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.Feedback.Services {

    /// <summary>
    /// Service class for working with feedback entries.
    /// </summary>
    public class FeedbackService {

        private readonly ILogger<FeedbackService> _logger;

        private readonly FeedbackDatabaseService _databaseService;

        #region Properties

        /// <summary>
        /// Gets a reference to a collection with the registered feedback plugins.
        /// </summary>
        protected FeedbackPluginCollection Plugins { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified dependencies.
        /// </summary>
        public FeedbackService(ILogger<FeedbackService> logger, FeedbackDatabaseService databaseService, FeedbackPluginCollection feedbackPlugins) {
            _logger = logger;
            _databaseService = databaseService;
            Plugins = feedbackPlugins;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the site with the specified <paramref name="key"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="key">The key (GUID) of the site.</param>
        /// <param name="site">When this method returns, holds the information about the site if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if a site was found; otherwise, <c>false</c>.</returns>
        public bool TryGetSite(Guid key, [NotNullWhen(true)] out FeedbackSiteSettings? site) {
            return Plugins.TryGetSite(key, out site);
        }

        /// <summary>
        /// Attempts to get the parent site of the specified <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The content representing a page under the site.</param>
        /// <param name="site">When this method returns, holds an instance of <see cref="FeedbackSiteSettings"/> representing the parent site if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
        public bool TryGetSite(IContent content, [NotNullWhen(true)] out FeedbackSiteSettings? site) {
            return Plugins.TryGetSite(content, out site);
        }

        /// <summary>
        /// Gets the user with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key (GUID) of the user.</param>
        /// <param name="user">When this method returns, holds an instance of <see cref="IFeedbackUser"/> if successful; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if a user was found; otherwise, <c>false</c>.</returns>
        public bool TryGetUser(Guid key, [NotNullWhen(true)] out IFeedbackUser? user) {
            foreach (var plugin in Plugins) {
                if (plugin.TryGetUser(key, out user)) {
                    return true;
                }
            }
            user = null;
            return false;
        }

        /// <summary>
        /// Returns an array of all feedback users.
        /// </summary>
        /// <returns>An array of <see cref="IFeedbackUser"/>.</returns>
        public IReadOnlyList<IFeedbackUser> GetUsers() {
            return Plugins.SelectMany(x => x.GetUsers()).Distinct().OrderBy(x => x.Name).ToArray();
        }

        /// <summary>
        /// Returns a paginated list of the feedback entries matching the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options describing the entries that should be returned.</param>
        /// <returns>An instance of <see cref="FeedbackEntryList"/>.</returns>
        public FeedbackEntryList GetEntries(FeedbackGetEntriesOptions options) {

            // Look up the entries in the database
            var entries = _databaseService
                .GetEntries(options, out int total)
                .Select(x => new FeedbackEntry(x))
                .ToArray();

            return new FeedbackEntryList(options.Page, options.PerPage, total, entries);

        }

        /// <summary>
        /// Returns a paginated list of entries for site with the specified <paramref name="siteKey"/>.
        /// </summary>
        /// <param name="siteKey">The key (GUID) of the site.</param>
        /// <param name="limit">The maxmium amount if items to be returned per page.</param>
        /// <param name="page">The page to be returned.</param>
        /// <returns>An instance of <see cref="FeedbackEntryList"/>.</returns>
        public FeedbackEntryList GetEntriesForSite(Guid siteKey, int limit = 0, int page = 0) {

            // Look up the entries in the database
            var entries = _databaseService
                .GetEntriesForSite(siteKey, limit, Math.Max(1, page), out int total)
                .Select(x => new FeedbackEntry(x))
                .ToArray();

            return new FeedbackEntryList(page, limit, total, entries);

        }

        /// <summary>
        /// Returns the entry with the specified <paramref name="key"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="key">The key (GUID) of the entry to be returned.</param>
        /// <returns>An instance of <see cref="FeedbackEntry"/> if successfull; otherwise, <c>null</c>.</returns>
        public FeedbackEntry? GetEntryByKey(Guid key) {

            FeedbackEntryDto? dto = _databaseService.GetEntryByKey(key);
            if (dto == null) return null;

            TryGetSite(dto.SiteKey, out var site);

            FeedbackRating? rating = null;
            site?.TryGetRating(dto.Rating, out rating);

            FeedbackStatus? status = null;
            site?.TryGetStatus(dto.Status, out status);

            // Normally "rating" and "status" will never be null, but if a configured rating or status is removed with
            // existing feedback still referencing them, we have a null reference, so we ensure to set some fallback
            // values should this be the case
            rating ??= new FeedbackRating(dto.Rating, "notFound");
            status ??= new FeedbackStatus(dto.Status, "notFound");

            TryGetUser(dto.AssignedTo, out IFeedbackUser? user);

            return new FeedbackEntry(dto, rating, status, user);

        }

        /// <summary>
        /// Archives the specified <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The entry to be archived.</param>
        public void Archive(FeedbackEntry entry) {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            entry.IsArchived = true;
            _databaseService.Update(entry._entry);
        }

        /// <summary>
        /// Deletes the specified <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The entry to be delete.</param>
        public void Delete(FeedbackEntry entry) {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            _databaseService.Delete(entry._entry);
        }

        /// <summary>
        /// Adds a new entry with the specified <paramref name="rating"/> for <paramref name="page"/>.
        /// </summary>
        /// <param name="site">The parent site.</param>
        /// <param name="page">The page for which the rating should be added.</param>
        /// <param name="rating">The rating.</param>
        /// <returns>An instance of <see cref="AddRatingResult"/>.</returns>
        public AddRatingResult AddRating(FeedbackSiteSettings site, IPublishedContent page, FeedbackRating rating) {

            if (site.Statuses.Count == 0) throw new FeedbackException($"Site with key {site.Key} does not specify any statuses.");

            // Initialize a new entry
            FeedbackEntry entry = new() {
                Key = Guid.NewGuid(),
                SiteKey = site.Key,
                PageKey = page.Key,
                Rating = rating,
                Status = site.Statuses.First(),
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            // Attempt to add the entry to the database
            try {

                // Trigger the "OnRatingSubmitting" before adding the entry
                foreach (IFeedbackPlugin plugin in Plugins) {
                    try {
                        if (!plugin.OnRatingSubmitting(this, entry)) {
                            return AddRatingResult.Cancelled("The feedback submission was cancelled by the server.");
                        }
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnRatingSubmitting.", plugin.GetType().FullName);
                    }
                }

                // Insert the item into the database
                _databaseService.Insert(entry.Dto);

                // Trigger the "OnRatingSubmitted" after the entry has been added
                foreach (IFeedbackPlugin plugin in Plugins) {
                    try {
                        plugin.OnRatingSubmitted(this, entry);
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnRatingSubmitted.", plugin.GetType().FullName);
                    }
                }

                return AddRatingResult.Success(entry);

            } catch (Exception ex) {

                _logger.LogError(ex, "Unable to add feedback entry.");

                return AddRatingResult.Cancelled("The feedback submission could not be saved due to an error on the server.");

            }

        }

        /// <summary>
        /// Updates (saves) the specified <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>An instance of <see cref="UpdateEntryResult"/>.</returns>
        public UpdateEntryResult UpdateEntry(FeedbackEntry entry) {

            // Ensure the string values are NULL (opposed to empty or white space)
            entry.Name = entry.Name.NullIfWhiteSpace();
            entry.Email = entry.Email.NullIfWhiteSpace();
            entry.Comment = entry.Comment.NullIfWhiteSpace();

            // Attempt to add the entry to the database
            try {

                // Trigger the "OnEntryUpdating" before adding the entry
                foreach (IFeedbackPlugin plugin in Plugins) {
                    try {
                        if (!plugin.OnEntryUpdating(this, entry)) {
                            return UpdateEntryResult.Cancelled("The feedback submission was cancelled by the server.");
                        }
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnEntryUpdating.", plugin.GetType().FullName);
                    }
                }

                // Insert the item into the database
                _databaseService.Update(entry.Dto);

                // Trigger the "OnEntryUpdated" after the entry has been added
                foreach (IFeedbackPlugin plugin in Plugins) {
                    try {
                        plugin.OnEntryUpdated(this, entry);
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnEntryUpdated.", plugin.GetType().FullName);
                    }
                }

                return UpdateEntryResult.Success(entry);

            } catch (Exception ex) {

                _logger.LogError(ex, "Unable to add feedback entry.");

                return UpdateEntryResult.Cancelled("The feedback submission could not be updated due to an error on the server.");

            }

        }

        /// <summary>
        /// Adds a new comment to <paramref name="page"/>.
        /// </summary>
        /// <param name="site">The parent site.</param>
        /// <param name="page">The page for which the rating should be added.</param>
        /// <param name="rating">The rating.</param>
        /// <param name="name">The name of the uset.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="comment">The comment of the user.</param>
        /// <returns>An instance of <see cref="AddCommentResult"/>.</returns>
        public AddCommentResult AddComment(FeedbackSiteSettings site, IPublishedContent page, FeedbackRating rating, string? name, string? email, string? comment) {

            if (site.Statuses.Count == 0) throw new FeedbackException($"Site with key {site.Key} does not specify any statuses.");

            FeedbackEntry entry = new() {
                Key = Guid.NewGuid(),
                SiteKey = site.Key,
                PageKey = page.Key,
                Rating = rating,
                Status = site.Statuses.First(),
                Name = name.NullIfWhiteSpace(),
                Email = email.NullIfWhiteSpace(),
                Comment = comment.NullIfWhiteSpace(),
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            // Attempt to add the entry to the database
            try {

                // Trigger the "OnEntrySubmitting" before adding the entry
                foreach (IFeedbackPlugin plugin in Plugins) {
                    try {
                        if (!plugin.OnEntrySubmitting(this, entry)) {
                            return AddCommentResult.Cancelled("The feedback submission was cancelled by the server.");
                        }
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnEntrySubmitting.", plugin.GetType().FullName);
                    }
                }

                // Insert the item into the database
                _databaseService.Insert(entry.Dto);

                // Trigger the "OnEntrySubmitted" after the entry has been added
                foreach (IFeedbackPlugin plugin in Plugins) {
                    try {
                        plugin.OnEntrySubmitted(this, entry);
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnEntrySubmitted.", plugin.GetType().FullName);
                    }
                }

                return AddCommentResult.Success(entry);

            } catch (Exception ex) {

                _logger.LogError(ex, "Unable to add feedback entry.");

                return AddCommentResult.Cancelled("The feedback submission could not be saved due to an error on the server.");

            }

        }

        /// <summary>
        /// Sets the assigned user of <paramref name="entry"/> to <paramref name="user"/>.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool SetAssignedTo(FeedbackEntry entry, IFeedbackUser? user) {

            // Some input validation
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            // Get the current (old) user
            IFeedbackUser? oldUser = entry.AssignedTo;
            IFeedbackUser? newUser = user;

            // Trigger the "OnUserAssigning" before assigning the user
            foreach (IFeedbackPlugin plugin in Plugins) {
                try {
                    if (!plugin.OnUserAssigning(this, entry, newUser)) return false;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnUserAssigning.", plugin.GetType().FullName);
                }
            }

            entry.AssignedTo = newUser;
            entry.UpdateDate = DateTime.UtcNow;

            _databaseService.Update(entry.Dto);

            // Trigger the "OnUserAssigned" event when the user has been assigned
            foreach (IFeedbackPlugin plugin in Plugins) {
                try {
                    plugin.OnUserAssigned(this, entry, oldUser, newUser);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnUserAssigned.", plugin.GetType().FullName);
                }
            }

            return true;

        }

        /// <summary>
        /// Sets the status of <paramref name="entry"/> to <paramref name="status"/>.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="status">The status.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool SetStatus(FeedbackEntry entry, FeedbackStatus status) {

            // Some input validation
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            // Get the current (old) status
            FeedbackStatus oldStatus = entry.Status;

            // Trigger the "OnStatusChanging" before chaging the status
            foreach (IFeedbackPlugin plugin in Plugins) {
                try {
                    if (!plugin.OnStatusChanging(this, entry, status)) return false;
                } catch (Exception ex) {
                    _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnStatusChanging.", plugin.GetType().FullName);
                }
            }

            entry.Status = status;
            entry.UpdateDate = DateTime.UtcNow;

            _databaseService.Update(entry.Dto);

            // Trigger the "OnStatusChanged" event when the user has been assigned
            foreach (IFeedbackPlugin plugin in Plugins) {
                try {
                    plugin.OnStatusChanged(this, entry, oldStatus, status);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Plugin of type {PluginType} failed for method OnStatusChanged.", plugin.GetType().FullName);
                }
            }

            return true;

        }

        #endregion

    }

}