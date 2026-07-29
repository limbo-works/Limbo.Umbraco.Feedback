using System;
using System.Text.Json.Serialization;
using Limbo.Umbraco.Feedback.Models.Ratings;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class RatingApiModel {

    #region Properties

    [JsonPropertyName("alias")]
    public string Alias { get; }

    [JsonPropertyName("key")]
    public Guid Key { get; }

    /// <summary>
    /// Gets the explicit name of the rating, or <c>null</c> if the rating doesn't specify one - in which case the
    /// back office falls back to the localized <c>feedback_rating{Alias}</c> key.
    /// </summary>
    /// <remarks>
    /// Prior to Umbraco 14, the name was localized server side through <c>ILocalizedTextService</c>. Package
    /// language files (<c>~/App_Plugins/*/Lang/*.xml</c>) are no longer loaded by Umbraco, so localization has
    /// moved to the client, where it is registered through a <c>localization</c> extension manifest.
    /// </remarks>
    [JsonPropertyName("name")]
    public string? Name { get; }

    [JsonPropertyName("active")]
    public bool IsActive { get; }

    #endregion

    #region Constructors

    public RatingApiModel(FeedbackRating rating) {
        Alias = rating.Alias;
        Key = rating.Key;
        Name = string.IsNullOrWhiteSpace(rating.Name) ? null : rating.Name;
        IsActive = rating.IsActive;
    }

    #endregion

}
