using Marten;

using Nine.Identities.Domain.Accounts.Repositories;
using Nine.Identities.Domain.Accounts.Services;
using Nine.Identities.Infrastructure.Accounts.Repositories;
using Nine.Identities.Infrastructure.Accounts.Services;
using Nine.Identities.Infrastructure.Marten;

namespace Nine.WebApi.Identities;

public static class IdentitiesInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentitiesInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services
            .AddMarten(options => IdentitiesMartenStoreOptions.Configure(options, connectionString))
            .UseLightweightSessions();

        services.AddScoped<IAccountCommandRepository, AccountCommandRepository>();
        services.AddScoped<IAccountEmailAddressUniquenessChecker, AccountEmailAddressUniquenessChecker>();

        return services;
    }
}
