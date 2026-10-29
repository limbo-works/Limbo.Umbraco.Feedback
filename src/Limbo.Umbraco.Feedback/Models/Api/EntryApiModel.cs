using System;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Models.Users;
using System.Text.Json.Serialization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Models.Api;

public class EntryApiModel {

    protected FeedbackEntry Entry { get; }

    [JsonPropertyName("id")]
    public int Id => Entry.Id;

    [JsonPropertyName("key")]
    public Guid Key => Entry.Key;

    [JsonPropertyName("site")]
    public SiteApiModel Site { get; }

    [JsonPropertyName("page")]
    public PageApiModel? Page { get; }

    [JsonPropertyName("name")]
    public string? Name => Entry.Name;

    [JsonPropertyName("email")]
    public string? Email => Entry.Email;

    [JsonPropertyName("comment")]
    public string? Comment => Entry.Comment;

    [JsonPropertyName("status")]
    public StatusApiModel? Status { get; }

    [JsonPropertyName("rating")]
    public RatingApiModel? Rating { get; }

    [JsonPropertyName("assignedTo")]
    public IFeedbackUser? AssignedTo { get; }

    [JsonPropertyName("createDate")]
    public DateTime CreateDate => Entry.CreateDate;

    [JsonPropertyName("updateDate")]
    public DateTime UpdateDate => Entry.UpdateDate;

    [JsonPropertyName("archived")]
    public bool IsArchived => Entry.IsArchived;

    public EntryApiModel(FeedbackEntry entry, SiteApiModel site, PageApiModel? page, StatusApiModel? status, RatingApiModel? rating, IFeedbackUser? assignedTo) {
        Entry = entry;
        Site = site;
        Page = page;
        Status = status;
        Rating = rating;
        AssignedTo = assignedTo;
    }

}