using Limbo.Umbraco.Feedback.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Management;

/// <summary>
/// Base class for the feedback management API controllers.
/// </summary>
/// <remarks>
/// Replaces the <c>UmbracoAuthorizedApiController</c> + <c>[PluginController]</c> combination used prior to
/// Umbraco 14. Endpoints are now served from <c>/umbraco/feedback/api/v1/*</c>, are authorized through the back
/// office access policy, and are described by their own OpenAPI document at
/// <c>/umbraco/swagger/feedback/swagger.json</c>.
/// </remarks>
[ApiController]
[BackOfficeRoute(FeedbackConstants.ApiRoute)]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[MapToApi(FeedbackConstants.ApiName)]
public abstract class FeedbackManagementControllerBase : ControllerBase { }
