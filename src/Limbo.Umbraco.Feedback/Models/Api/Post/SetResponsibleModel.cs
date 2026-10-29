using System;
using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api.Post;

public class SetResponsibleModel {

    /// <summary>
    /// Gets or sets the key of the user the entry should be assigned to, or <see cref="Guid.Empty"/> to clear the
    /// assignment.
    /// </summary>
    [JsonPropertyName("responsible")]
    public Guid Responsible { get; set; }

}
