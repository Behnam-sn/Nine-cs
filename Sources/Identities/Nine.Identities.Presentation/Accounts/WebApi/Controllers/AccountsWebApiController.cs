using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nine.Identities.Application.Accounts.Commands.CreateAccountWithPassword;
using Nine.Identities.Presentation.Accounts.WebApi.Requests;
using Nine.Identities.Presentation.Accounts.WebApi.Responses;
using Nine.Identities.Presentation.Common.WebApi.Controllers;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Presentation.Accounts.WebApi.Controllers;

[ApiVersion(1.0)]
public sealed class AccountsWebApiController : WebApiController
{
    public AccountsWebApiController(ICommandBus commandBus, IQueryBus queryBus) : base(commandBus, queryBus)
    {
    }

    [HttpPost]
    public async Task<ActionResult<CreateAccountWithPasswordResponseV1>> CreateWithPassword([FromBody] CreateAccountWithPasswordRequestV1 request, CancellationToken cancellationToken)
    {
        var command = new CreateAccountWithPasswordCommandV1(
            EmailAddress: request.EmailAddress,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber
        );

        var accountId = await CommandBus.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateAccountWithPasswordResponseV1(accountId.ToString())
        );
    }
}