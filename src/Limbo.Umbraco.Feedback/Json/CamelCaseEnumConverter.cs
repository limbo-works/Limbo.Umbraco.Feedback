using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limbo.Umbraco.Feedback.Json;

/// <summary>
/// JSON converter that serializes <typeparamref name="T"/> as a camel cased string.
/// </summary>
/// <typeparam name="T">The type of the enum.</typeparam>
/// <remarks>
/// Replaces the <c>EnumCamelCaseConverter</c> from <c>Skybrud.Essentials.Json.Newtonsoft</c>, as the package no
/// longer depends on <c>Newtonsoft.Json</c> - Umbraco 14+ serializes both the management API and the front-end
/// APIs with <c>System.Text.Json</c>.
/// </remarks>
public class CamelCaseEnumConverter<T> : JsonStringEnumConverter<T> where T : struct, Enum {

    /// <summary>
    /// Initializes a new instance of the converter.
    /// </summary>
    public CamelCaseEnumConverter() : base(JsonNamingPolicy.CamelCase) { }

}
