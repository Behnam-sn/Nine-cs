using Asp.Versioning;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Nine.Identity.Domain.Users.Entities;
using Nine.Identity.Infrastructure.Identity;
using Nine.Identity.Presentation.Users.WebApi.ExceptionHandlers;

using OpenIddict.Validation.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Nine.WebApi.Configurations;

public static class IdentityConfiguration
{
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        AddPresentation(services);
        AddInfrastructure(services);
        services.AddResourceApiAuthorization(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        return services;
    }

    private static void AddPresentation(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(Identity.Presentation.AssemblyReference.Assembly);

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

    private static void AddInfrastructure(IServiceCollection services)
    {
        services.AddDbContext<IdentityContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("Identity");
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));
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
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityContext>();
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

                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, IdentityAuthSeeder.ApiScope);

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

        services.AddHostedService<IdentityAuthSeeder>();
    }
}
