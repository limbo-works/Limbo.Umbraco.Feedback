using System.Collections.Generic;
using Asp.Versioning;
using Limbo.Umbraco.Feedback.Constants;
using Limbo.Umbraco.Feedback.Models.Users;
using Limbo.Umbraco.Feedback.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable 1591

namespace Limbo.Umbraco.Feedback.Controllers.Management;

/// <summary>
/// Endpoint returning the users a feedback entry may be assigned to.
/// </summary>
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = FeedbackConstants.ApiName)]
public class FeedbackUsersController : FeedbackManagementControllerBase {

    private readonly FeedbackService _feedbackService;

    public FeedbackUsersController(FeedbackService feedbackService) {
        _feedbackService = feedbackService;
    }

    [HttpGet("users")]
    [ProducesResponseType<IEnumerable<FeedbackUser>>(StatusCodes.Status200OK)]
    public IActionResult GetUsers() {
        return Ok(_feedbackService.GetUsers());
    }

}
