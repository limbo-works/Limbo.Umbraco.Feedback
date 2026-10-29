using System;
using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api.Post;

public class SetStatusModel {

    [JsonPropertyName("status")]
    public Guid Status { get; set; }

}
