using System;
using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api.Post;

public class AddCommentModel {

    [JsonPropertyName("siteKey")]
    public Guid SiteKey { get; set; }

    [JsonPropertyName("pageKey")]
    public Guid PageKey { get; set; }

    [JsonPropertyName("rating")]
    public Guid Rating { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

}