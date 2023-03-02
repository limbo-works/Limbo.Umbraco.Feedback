using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Models.Fields;
using Limbo.Umbraco.Feedback.Models.Ratings;
using Limbo.Umbraco.Feedback.Models.Statuses;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.Feedback.Models.Sites {

    /// <summary>
    /// Class with information about a site.
    /// </summary>
    public class FeedbackSiteSettings {

        #region Properties

        /// <summary>
        /// Gets the numeric ID of the site.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the key (GUID) of the site.
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the available ratings for this site.
        /// </summary>
        public IReadOnlyList<FeedbackRating> Ratings { get; }

        /// <summary>
        /// Gets the available statuses for this site.
        /// </summary>
        public IReadOnlyList<FeedbackStatus> Statuses { get; }

        /// <summary>
        /// Gets the configuration for the fields in the feedback form.
        /// </summary>
        public FeedbackFields Fields { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="site"/>.
        /// </summary>
        /// <param name="site">An instance of <see cref="IPublishedContent"/> representing the site.</param>
        public FeedbackSiteSettings(IPublishedContent site) {

            Id = site.Id;
            Key = site.Key;
            Name = site.Name!;

            Ratings = new[] { FeedbackConstants.Ratings.Positive, FeedbackConstants.Ratings.Negative };

            Statuses = new[] { FeedbackConstants.Statuses.New, FeedbackConstants.Statuses.InProgress, FeedbackConstants.Statuses.Closed };

            Fields = new FeedbackFields();

        }

        #endregion

        #region Member methods

        /// <summary>
        /// Gets the rating with the specified <paramref name="key"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="key">The key (GUID) of the rating.</param>
        /// <param name="rating">When this method returns, holds an instance of <see cref="FeedbackRating"/> if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetRating(Guid key, [NotNullWhen(true)] out FeedbackRating? rating) {
            rating = Ratings.FirstOrDefault(x => x.Key == key);
            return rating != null;
        }

        /// <summary>
        /// Gets the status with the specified <paramref name="key"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="key">The key (GUID) of the status.</param>
        /// <param name="status">When this method returns, holds an instance of <see cref="FeedbackStatus"/> if successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetStatus(Guid key, [NotNullWhen(true)] out FeedbackStatus? status) {
            status = Statuses.FirstOrDefault(x => x.Key == key);
            return status != null;
        }

        #endregion

    }

}