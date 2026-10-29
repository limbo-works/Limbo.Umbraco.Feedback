using System.Text.Json.Serialization;
using Limbo.Umbraco.Feedback.Json;

namespace Limbo.Umbraco.Feedback.Services;

/// <summary>
/// Enum class indicating the sort order of a list of entries.
/// </summary>
[JsonConverter(typeof(CamelCaseEnumConverter<EntriesSortOrder>))]
public enum EntriesSortOrder {

    /// <summary>
    /// Indicates that entries should be sorted in ascending order.
    /// </summary>
    Asc,

    /// <summary>
    /// Indicates that entries should be sorted in descending order.
    /// </summary>
    Desc

}