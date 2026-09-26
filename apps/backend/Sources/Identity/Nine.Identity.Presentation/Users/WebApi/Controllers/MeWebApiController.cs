using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Nine.Identity.Presentation.Users.WebApi.Responses;
using Nine.Shared.Application.Abstractions.Messaging;
using Nine.Shared.Application.Common.Security;
using Nine.Shared.Presentation.Common.Security;
using Nine.Shared.Presentation.Common.WebApi.Controllers;

namespace Nine.Identity.Presentation.Users.WebApi.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/me")]
[Authorize]
public sealed class MeWebApiController : WebApiController
{
    public MeWebApiController(ICommandBus commandBus, IQueryBus queryBus)
        : base(commandBus, queryBus)
    {
    }

    [HttpGet]
    public ActionResult<MeResponseV1> Get()
    {
        var userId = User.FindUserId() ?? string.Empty;
        var email = User.FindEmail();

        return Ok(new MeResponseV1(userId, email));
    }
}
