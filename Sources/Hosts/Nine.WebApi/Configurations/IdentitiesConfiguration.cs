using Asp.Versioning;

using Marten;

using Nine.Identities.Domain.Accounts.Repositories;
using Nine.Identities.Domain.Accounts.Services;
using Nine.Identities.Infrastructure.Accounts.Repositories;
using Nine.Identities.Infrastructure.Accounts.Services;
using Nine.Identities.Infrastructure.Marten;
using Nine.Identities.Presentation.Accounts.WebApi.Controllers;
using Nine.Identities.Presentation.Accounts.WebApi.ExceptionHandlers;

namespace Nine.WebApi.Configurations;

public static class IdentitiesConfiguration
{
    public static IServiceCollection AddIdentities(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        AddPresentation(services);
        AddInfrastructure(services, connectionString);

        return services;
    }

    private static void AddPresentation(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(AccountsWebApiController).Assembly);

        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc();

        services.AddExceptionHandler<AccountExceptionHandler>();
        services.AddProblemDetails();
    }

    private static void AddInfrastructure(IServiceCollection services, string connectionString)
    {
        services
            .AddMarten(options => IdentitiesMartenStoreOptions.Configure(options, connectionString))
            .UseLightweightSessions();

        services.AddScoped<IAccountCommandRepository, AccountCommandRepository>();
        services.AddScoped<IAccountEmailAddressUniquenessChecker, AccountEmailAddressUniquenessChecker>();
        services.AddScoped<IAccountPhoneNumberUniquenessChecker, AccountPhoneNumberUniquenessChecker>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
    }
}
