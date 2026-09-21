using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Nine.SharedKernel.Common.Security;

using OpenIddict.Abstractions;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Nine.Identities.Infrastructure.Identity;

public sealed class IdentitiesAuthSeeder : IHostedService
{
    public const string ResourceOwnerClientId = "nine-resource-owner";
    public const string SpaClientId = "nine-spa";
    public const string ApiScope = "api";

    private readonly IServiceProvider _serviceProvider;

    public IdentitiesAuthSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentitiesDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await EnsureRoleAsync(roleManager, RoleNames.Member);
        await EnsureRoleAsync(roleManager, RoleNames.Moderator);

        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopeManager.FindByNameAsync(ApiScope, cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = ApiScope,
                DisplayName = "Nine API"
            }, cancellationToken);
        }

        if (await applicationManager.FindByClientIdAsync(ResourceOwnerClientId, cancellationToken) is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = ResourceOwnerClientId,
                DisplayName = "Nine resource-owner (development)",
                ClientType = ClientTypes.Public,
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + ApiScope
                }
            }, cancellationToken);
        }

        if (await applicationManager.FindByClientIdAsync(SpaClientId, cancellationToken) is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = SpaClientId,
                DisplayName = "Nine SPA",
                ClientType = ClientTypes.Public,
                ConsentType = ConsentTypes.Implicit,
                RedirectUris =
                {
                    new Uri("http://localhost:5173/callback"),
                    new Uri("https://localhost:5173/callback")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:5173/"),
                    new Uri("https://localhost:5173/")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.EndSession,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + ApiScope
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName) { Id = Guid.NewGuid() });
        }
    }
}
