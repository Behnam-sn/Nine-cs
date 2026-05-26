using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nine.Identities.Application.Accounts.Commands.CreateAccountWithPassword;
using Nine.Identities.Presentation.Accounts.WebApi.Requests;
using Nine.Identities.Presentation.Accounts.WebApi.Responses;
using Nine.Identities.Presentation.Common.WebApi.Controllers;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Presentation.Accounts.WebApi.Controllers;

[Route("accounts")]
public sealed class AccountWebApiController : WebApiController
{
    public AccountWebApiController(ICommandBus commandBus, IQueryBus queryBus) : base(commandBus, queryBus)
    {
    }

    [HttpPost]
    public async Task<ActionResult<CreateAccountWithPasswordResponse>> CreateWithPassword([FromBody] CreateAccountWithPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAccountWithPasswordCommand(
            EmailAddress: request.EmailAddress,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber
        );

        var accountId = await CommandBus.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateAccountWithPasswordResponse(accountId.ToString())
        );
    }
}