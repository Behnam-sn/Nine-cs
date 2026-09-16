using Asp.Versioning;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Nine.Identities.Domain.Users.Entities;
using Nine.Identities.Infrastructure.Identity;
using Nine.Identities.Presentation.Users.WebApi.Controllers;
using Nine.Identities.Presentation.Users.WebApi.ExceptionHandlers;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
            .AddApplicationPart(Identities.Presentation.AssemblyReference.Assembly);

        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc();

        services.AddExceptionHandler<UserExceptionHandler>();
        services.AddProblemDetails();
    }

    private static void AddInfrastructure(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentitiesDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identities"));
            options.UseOpenIddict();
        });

        services
            .AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<IdentitiesDbContext>()
            .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentitiesDbContext>();
            })
            .AddServer(options =>
            {
                options
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetTokenEndpointUris("connect/token")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetUserInfoEndpointUris("connect/userinfo");

                options
                    .AllowAuthorizationCodeFlow()
                    .AllowPasswordFlow()
                    .AllowRefreshTokenFlow();

                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, IdentitiesAuthSeeder.ApiScope);

                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.DisableAccessTokenEncryption();
                options.DisableAudienceValidation();
                options.DisableResourceValidation();

                options
                    .SetAccessTokenLifetime(TimeSpan.FromMinutes(15))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(14));

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddHostedService<IdentitiesAuthSeeder>();
    }
}
