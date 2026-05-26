using Microsoft.AspNetCore.Mvc;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Presentation.Common.WebApi.Controllers;

[ApiController]
public abstract class WebApiController : ControllerBase
{
    protected readonly ICommandBus CommandBus;
    protected readonly IQueryBus QueryBus;

    protected WebApiController(ICommandBus commandBus, IQueryBus queryBus)
    {
        CommandBus = commandBus;
        QueryBus = queryBus;
    }
}
