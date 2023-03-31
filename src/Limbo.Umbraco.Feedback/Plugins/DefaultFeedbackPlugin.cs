namespace Limbo.Umbraco.Feedback.Plugins {

    /// <summary>
    /// Default feedback plugin.
    /// </summary>
    public class DefaultFeedbackPlugin : FeedbackPluginBase {

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="dependencies"/>.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        public DefaultFeedbackPlugin(FeedbackPluginDependencies dependencies) : base(dependencies) { }

    }

}