using System.Net;
using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Services;

namespace Limbo.Umbraco.Feedback.Events {

    /// <summary>
    /// Events args for when a new feedback entry is being added.
    /// </summary>
    public class EntryAddingEventArgs {

        /// <summary>
        /// Gets or sets whether the event should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the message returned to the user.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The HTTP status code that should be used for the response. If not specified, and <see cref="Cancel"/> is
        /// <see langword="true"/>, <see cref="HttpStatusCode.BadRequest"/> will be used as fallback.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Gets a reference to the current <see cref="FeedbackService"/>.
        /// </summary>
        public FeedbackService Service { get; }

        /// <summary>
        /// Gets a reference to the feedback entry being added.
        /// </summary>
        public FeedbackEntry Entry { get; }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="feedbackService"/> and <paramref name="entry"/>.
        /// </summary>
        /// <param name="feedbackService">The current <see cref="FeedbackService"/>.</param>
        /// <param name="entry">The feedback entry being added.</param>
        public EntryAddingEventArgs(FeedbackService feedbackService, FeedbackEntry entry) {
            Service = feedbackService;
            Entry = entry;
        }

    }

}