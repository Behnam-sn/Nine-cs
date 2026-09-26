using Nine.Shared.Application.Abstractions.Messaging;
using Nine.Shared.Infrastructure.Messaging;

namespace Nine.WebApi.Configurations;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(Identity.Application.AssemblyReference.Assembly));

        services.AddScoped<ICommandBus, CommandBus>();
        services.AddScoped<IQueryBus, QueryBus>();

        return services;
    }
}
