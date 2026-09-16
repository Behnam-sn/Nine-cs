using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nine.Identities.Presentation.Common.WebApi.Controllers;
using Nine.Identities.Presentation.Users.WebApi.Responses;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Presentation.Users.WebApi.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/me")]
[Authorize(AuthenticationSchemes = "OpenIddict.Validation.AspNetCore")]
public sealed class MeWebApiController : WebApiController
{
    public MeWebApiController(ICommandBus commandBus, IQueryBus queryBus)
        : base(commandBus, queryBus)
    {
    }

    [HttpGet]
    public ActionResult<MeResponseV1> Get()
    {
        var userId = User.FindFirstValue("sub")
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? string.Empty;

        var email = User.FindFirstValue("email")
                    ?? User.FindFirstValue(ClaimTypes.Email);

        return Ok(new MeResponseV1(userId, email));
    }
}
