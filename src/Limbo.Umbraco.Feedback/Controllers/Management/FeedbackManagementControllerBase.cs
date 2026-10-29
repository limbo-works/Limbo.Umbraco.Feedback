using Limbo.Umbraco.Feedback.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Management;

/// <summary>
/// Base class for the feedback management API controllers.
/// </summary>
[ApiController]
[VersionedApiBackOfficeRoute(FeedbackApiConstants.Route)]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
[MapToApi(FeedbackApiConstants.Alias)]
public abstract class FeedbackManagementControllerBase : ControllerBase;