using Umbraco.Cms.Core.Composing;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Plugins {

    public class FeedbackPluginCollectionBuilder : OrderedCollectionBuilderBase<FeedbackPluginCollectionBuilder, FeedbackPluginCollection, IFeedbackPlugin> {

        protected override FeedbackPluginCollectionBuilder This => this;

    }

}