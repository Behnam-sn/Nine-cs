using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identity.Application.Authentication.Queries.GetUserForSignIn;

public sealed record GetUserForSignInQueryV1(string UserId) : IQuery<AuthenticateUserResult>;
