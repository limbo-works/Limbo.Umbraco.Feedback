using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Limbo.Umbraco.Feedback.Models.Sites;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class SiteApiModel {

    [JsonPropertyName("id")]
    public int Id { get; }

    [JsonPropertyName("key")]
    public Guid Key { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("ratings")]
    public IReadOnlyList<RatingApiModel> Ratings { get; }

    [JsonPropertyName("statuses")]
    public IReadOnlyList<StatusApiModel> Statuses { get; }

    public SiteApiModel(FeedbackSiteSettings site) {
        Id = site.Id;
        Key = site.Key;
        Name = site.Name;
        Ratings = site.Ratings.Select(x => new RatingApiModel(x)).ToArray();
        Statuses = site.Statuses.Select(x => new StatusApiModel(x)).ToArray();
    }

}
