using System.Collections.Generic;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Limbo.Umbraco.Feedback.Config {

    /// <summary>
    /// Class representing the settings for the feedback module.
    /// </summary>
    public class FeedbackSettings {

        /// <summary>
        /// Gets or sets whether <see cref="DisableDefaultPlugin"/> should be disabled.
        /// </summary>
        public bool DisableDefaultPlugin { get; private set; }

        /// <summary>
        /// Gets a list of content type aliases that represent a site node.
        /// </summary>
        public HashSet<string> SiteContentTypes { get; private set; } = new();

        /// <summary>
        /// Gets a list of content type aliases that represent a page node.
        /// </summary>
        public HashSet<string> PageContentTypes { get; private set; } = new();

    }

}