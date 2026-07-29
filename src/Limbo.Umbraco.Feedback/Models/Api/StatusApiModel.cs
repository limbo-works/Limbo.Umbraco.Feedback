using System;
using System.Text.Json.Serialization;
using Limbo.Umbraco.Feedback.Models.Statuses;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class StatusApiModel {

    #region Properties

    [JsonPropertyName("alias")]
    public string Alias { get; }

    [JsonPropertyName("key")]
    public Guid Key { get; }

    /// <summary>
    /// Gets the explicit name of the status, or <c>null</c> if the status doesn't specify one - in which case the
    /// back office falls back to the localized <c>feedback_status{Alias}</c> key.
    /// </summary>
    /// <remarks>See the remarks on <see cref="RatingApiModel.Name"/>.</remarks>
    [JsonPropertyName("name")]
    public string? Name { get; }

    [JsonPropertyName("active")]
    public bool IsActive { get; }

    #endregion

    #region Constructors

    public StatusApiModel(FeedbackStatus status) {
        Alias = status.Alias;
        Key = status.Key;
        Name = string.IsNullOrWhiteSpace(status.Name) ? null : status.Name;
        IsActive = status.IsActive;
    }

    #endregion

}
