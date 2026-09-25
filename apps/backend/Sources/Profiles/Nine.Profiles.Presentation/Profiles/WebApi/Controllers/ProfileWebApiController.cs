using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Nine.Profiles.Application.Profiles.Commands.Create;
using Nine.Profiles.Presentation.Profiles.WebApi.Requests;
using Nine.Profiles.Presentation.Profiles.WebApi.Responses;
using Nine.SharedKernel.Abstractions.Messaging;
using Nine.SharedKernel.Common.Security;
using Nine.SharedKernel.Common.WebApi.Controllers;

namespace Nine.Profiles.Presentation.Profiles.WebApi.Controllers;

[ApiVersion(1.0)]
public sealed class ProfileWebApiController : WebApiController
{
    public ProfileWebApiController(ICommandBus commandBus, IQueryBus queryBus) : base(commandBus, queryBus)
    {
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.MustHaveVerifiedEmail)]
    [Authorize(Policy = AuthorizationPolicies.MustBeMember)]
    public async Task<ActionResult<CreateProfileResponseV1>> Create(
        [FromBody] CreateProfileRequestV1 request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindUserId();
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }

        var command = new CreateProfileCommandV1(
            Name: request.Name,
            Handle: request.Handle,
            AvatarObjectKey: request.AvatarObjectKey,
            AvatarMediaType: request.AvatarMediaType,
            Bio: request.Bio,
            OwnerId: ownerId
        );

        var profileId = await CommandBus.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateProfileResponseV1(profileId.ToString())
        );
    }
}
