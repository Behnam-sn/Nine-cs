using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Application.Authentication.Queries.GetUserForSignIn;

public sealed record GetUserForSignInQueryV1(string UserId) : IQuery<AuthenticateUserResult>;
