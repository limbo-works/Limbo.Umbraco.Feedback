using Limbo.Umbraco.Feedback.Models.Entries;
using Limbo.Umbraco.Feedback.Services;

namespace Limbo.Umbraco.Feedback.Events {

    /// <summary>
    /// Events args for when a new feedback entry has been added.
    /// </summary>
    public class EntryAddedEventArgs {

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
        public EntryAddedEventArgs(FeedbackService feedbackService, FeedbackEntry entry) {
            Service = feedbackService;
            Entry = entry;
        }

    }

}