using System;
using System.Text.Json.Serialization;

namespace Limbo.Umbraco.Feedback.Models.Users;

/// <summary>
/// Interface describing a feedback user.
/// </summary>
public interface IFeedbackUser {

    /// <summary>
    /// Gets the ID of the user.
    /// </summary>
    [JsonPropertyName("id")]
    int Id { get; }

    /// <summary>
    /// Gets the key (GUID) of the user.
    /// </summary>
    [JsonPropertyName("key")]
    Guid Key { get; }

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    [JsonPropertyName("name")]
    string Name { get; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [JsonPropertyName("email")]
    string Email { get; }

    /// <summary>
    /// Gets the description of the user.
    /// </summary>
    [JsonPropertyName("description")]
    string? Description { get; }

    /// <summary>
    /// Gets the avatar of the user.
    /// </summary>
    [JsonPropertyName("avatar")]
    string? Avatar { get; }

    /// <summary>
    /// Gets the language of the user.
    /// </summary>
    [JsonPropertyName("language")]
    string Language { get; }

}