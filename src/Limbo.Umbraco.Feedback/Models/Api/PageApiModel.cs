using System;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class PageApiModel {

    [JsonPropertyName("id")]
    public int Id { get; }

    [JsonPropertyName("key")]
    public Guid Key { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("published")]
    public bool IsPublished { get; }

    [JsonPropertyName("url")]
    public string? Url { get; }

    public PageApiModel(IPublishedContent content, string? url) {
        Id = content.Id;
        Key = content.Key;
        Name = content.Name;
        IsPublished = true;
        Url = url;
    }

    public PageApiModel(IContent content) {
        Id = content.Id;
        Key = content.Key;
        Name = content.Name!;
        IsPublished = content.Published;
    }

}
