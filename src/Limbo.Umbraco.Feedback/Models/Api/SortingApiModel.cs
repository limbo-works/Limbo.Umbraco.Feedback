using System.Text.Json.Serialization;
using Limbo.Umbraco.Feedback.Services;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class SortingApiModel {

    [JsonPropertyName("field")]
    public EntriesSortField Field { get; init; }

    [JsonPropertyName("order")]
    public EntriesSortOrder Order { get; init; }

}
