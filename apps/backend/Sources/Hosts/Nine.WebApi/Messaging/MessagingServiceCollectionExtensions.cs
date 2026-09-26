using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.WebApi.Messaging;

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
