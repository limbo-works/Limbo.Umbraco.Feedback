using System;
using Asp.Versioning;
using Limbo.Umbraco.Feedback.Constants;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Composers;

/// <summary>
/// Registers the OpenAPI (Swagger) document for the feedback management API.
/// </summary>
/// <remarks>
/// New in the Umbraco 17 version of this package. The back office client is typed after this document, which is
/// served from <c>/umbraco/swagger/feedback/swagger.json</c>.
/// </remarks>
public class FeedbackApiComposer : IComposer {

    public void Compose(IUmbracoBuilder builder) {

        builder.Services.AddSingleton<IOperationIdHandler, FeedbackOperationIdHandler>();

        builder.Services.Configure<SwaggerGenOptions>(options => {

            options.SwaggerDoc(FeedbackConstants.ApiName, new OpenApiInfo {
                Title = "Limbo Feedback Management API",
                Version = "1.0",
                Description = "API used by the feedback workspace view in the Umbraco back office."
            });

            options.OperationFilter<FeedbackSecurityFilter>();

        });

    }

    /// <summary>
    /// Applies the back office security requirements to the endpoints of the feedback API.
    /// </summary>
    public class FeedbackSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase {
        protected override string ApiName => FeedbackConstants.ApiName;
    }

    /// <summary>
    /// Produces short operation IDs (the action name) for the feedback endpoints.
    /// </summary>
    public class FeedbackOperationIdHandler : OperationIdHandler {

        public FeedbackOperationIdHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) : base(apiVersioningOptions) { }

        protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor) {
            return controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith(
                "Limbo.Umbraco.Feedback.Controllers.Management", StringComparison.InvariantCultureIgnoreCase) is true;
        }

        public override string Handle(ApiDescription apiDescription) {
            return $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
        }

    }

}
