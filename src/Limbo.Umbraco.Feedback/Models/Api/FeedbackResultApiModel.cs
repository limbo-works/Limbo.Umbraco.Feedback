using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

/// <summary>
/// The response returned when looking up the feedback entries of a site or a page.
/// </summary>
public class FeedbackResultApiModel {

    [JsonPropertyName("site")]
    public required SiteApiModel Site { get; init; }

    [JsonPropertyName("entries")]
    public required EntryListApiModel Entries { get; init; }

}
