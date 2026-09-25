using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nine.Identities.Application.Users.Commands.CreateUserWithPassword;
using Nine.Identities.Presentation.Users.WebApi.Requests;
using Nine.Identities.Presentation.Users.WebApi.Responses;
using Nine.SharedKernel.Abstractions.Messaging;
using Nine.SharedKernel.Common.WebApi.Controllers;

namespace Nine.Identities.Presentation.Users.WebApi.Controllers;

[ApiVersion(1.0)]
public sealed class UsersWebApiController : WebApiController
{
    public UsersWebApiController(ICommandBus commandBus, IQueryBus queryBus) : base(commandBus, queryBus)
    {
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateUserWithPasswordResponseV1>> CreateWithPassword([FromBody] CreateUserWithPasswordRequestV1 request, CancellationToken cancellationToken)
    {
        var command = new CreateUserWithPasswordCommandV1(
            EmailAddress: request.EmailAddress,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber
        );

        var userId = await CommandBus.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateUserWithPasswordResponseV1(userId.ToString())
        );
    }
}