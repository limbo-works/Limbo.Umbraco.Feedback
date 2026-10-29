using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class EntryListApiModel {

    [JsonPropertyName("pagination")]
    public required PaginationApiModel Pagination { get; init; }

    [JsonPropertyName("sorting")]
    public required SortingApiModel Sorting { get; init; }

    [JsonPropertyName("data")]
    public required IReadOnlyList<EntryApiModel> Data { get; init; }

}
