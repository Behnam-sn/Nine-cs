using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Nine.Identities.Presentation.Users.WebApi.Responses;
using Nine.SharedKernel.Abstractions.Messaging;
using Nine.SharedKernel.Common.Security;
using Nine.SharedKernel.Common.WebApi.Controllers;

namespace Nine.Identities.Presentation.Users.WebApi.Controllers;

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
