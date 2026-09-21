using Microsoft.Extensions.DependencyInjection;

namespace Nine.SharedKernel.Common.Security;

public static class ResourceApiAuthorizationExtensions
{
    public static IServiceCollection AddResourceApiAuthorization(
        this IServiceCollection services,
        string authenticationScheme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authenticationScheme);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Authenticated, policy =>
            {
                policy.AuthenticationSchemes.Add(authenticationScheme);
                policy.RequireAuthenticatedUser();
            });

            options.AddPolicy(AuthorizationPolicies.MustHaveVerifiedEmail, policy =>
            {
                policy.AuthenticationSchemes.Add(authenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context => context.User.HasVerifiedEmail());
            });

            options.AddPolicy(AuthorizationPolicies.MustBeMember, policy =>
            {
                policy.AuthenticationSchemes.Add(authenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(RoleNames.Member);
                policy.RequireAssertion(context => !context.User.IsInRole(RoleNames.Moderator));
            });

            options.AddPolicy(AuthorizationPolicies.MustBeModerator, policy =>
            {
                policy.AuthenticationSchemes.Add(authenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(RoleNames.Moderator);
            });

            options.DefaultPolicy = options.GetPolicy(AuthorizationPolicies.Authenticated)!;
        });

        return services;
    }
}
