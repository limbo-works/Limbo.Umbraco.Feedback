using Limbo.Umbraco.Feedback.Events;
using System.Net;
using Limbo.Umbraco.Feedback.Models.Entries;

namespace Limbo.Umbraco.Feedback.Models.Results;

/// <summary>
/// Class representing the result when updating a feedback comment.
/// </summary>
public class UpdateEntryResult {

    #region Properties

    /// <summary>
    /// Gets the status of the result.
    /// </summary>
    public UpdateEntryStatus Status { get; }

    /// <summary>
    /// Gets the HTTP status code associated with this result.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets a reference to the entry.
    /// </summary>
    public FeedbackEntry? Entry { get; }

    /// <summary>
    /// Gets the message of the result - e.g. an error message.
    /// </summary>
    public string? Message { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance based on the specified <paramref name="status"/>, <paramref name="entry"/> and <paramref name="message"/>.
    /// </summary>
    /// <param name="status">The status of the result.</param>
    /// <param name="statusCode">The HTTP status code associated with the result.</param>
    /// <param name="entry">The entry.</param>
    /// <param name="message">The message of the result - e.g. an error message.</param>
    public UpdateEntryResult(UpdateEntryStatus status, HttpStatusCode statusCode, FeedbackEntry? entry, string? message) {
        Status = status;
        StatusCode = statusCode;
        Entry = entry;
        Message = message;
    }

    #endregion

    #region Static methods

    /// <summary>
    /// Initializes a new <see cref="UpdateEntryStatus.Failed"/> result.
    /// </summary>
    /// <param name="message">An error message about why the comment could not be added.</param>
    /// <returns>An instance of <see cref="UpdateEntryResult"/></returns>
    public static UpdateEntryResult Failed(string message) {
        return new UpdateEntryResult(UpdateEntryStatus.Failed, HttpStatusCode.InternalServerError, null, message);
    }

    /// <summary>
    /// Initializes a new <see cref="UpdateEntryStatus.Cancelled"/> result.
    /// </summary>
    /// <param name="message">A message about why adding the comment was cancelled.</param>
    /// <returns>An instance of <see cref="UpdateEntryResult"/></returns>
    public static UpdateEntryResult Cancelled(string message) {
        return new UpdateEntryResult(UpdateEntryStatus.Cancelled, HttpStatusCode.BadRequest, null, message);
    }

    /// <summary>
    /// Initializes a new <see cref="UpdateEntryStatus.Cancelled"/> result.
    /// </summary>
    /// <param name="args"></param>
    /// <param name="message">A message about why adding the comment was cancelled.</param>
    /// <returns>An instance of <see cref="UpdateEntryStatus"/></returns>
    public static UpdateEntryResult Cancelled(EntryUpdatingEventArgs args, string message) {
        if (!string.IsNullOrWhiteSpace(args.Message)) message = args.Message;
        return new UpdateEntryResult(UpdateEntryStatus.Cancelled, args.StatusCode ?? HttpStatusCode.BadRequest, null, message);
    }

    /// <summary>
    /// Initializes a new <see cref="UpdateEntryStatus.Success"/> result.
    /// </summary>
    /// <param name="entry">The entry that was added.</param>
    /// <returns>An instance of <see cref="UpdateEntryResult"/></returns>
    public static UpdateEntryResult Success(FeedbackEntry entry) {
        return new UpdateEntryResult(UpdateEntryStatus.Success, HttpStatusCode.OK, entry, null);
    }

    #endregion

}