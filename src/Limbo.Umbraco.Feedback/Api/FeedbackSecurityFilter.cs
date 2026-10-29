#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Umbraco.Cms.Api.Management.OpenApi;

namespace Limbo.Umbraco.Feedback.Api;

public class FeedbackSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase {

    protected override string ApiName => FeedbackApiConstants.Name;

}