#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Limbo.Umbraco.Feedback.Api;

public class FeedbackSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions> {
    public void Configure(SwaggerGenOptions options) {
        options.SwaggerDoc(FeedbackApiConstants.Alias, new OpenApiInfo {
            Title = FeedbackApiConstants.Name,
            Version = "1.0"
        });
        options.OperationFilter<FeedbackSecurityFilter>();
    }
}