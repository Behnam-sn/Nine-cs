using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Nine.Identity.Domain.Contracts.Users.Exceptions;
using Nine.Identity.Domain.Contracts.Users.ValueObjects;
using Nine.Identity.Domain.Users.Entities;
using Nine.SharedKernel.Abstractions.Messaging;
using Nine.SharedKernel.Common.Security;

namespace Nine.Identity.Application.Users.Commands.CreateUserWithPassword;

public sealed class CreateUserWithPasswordCommandV1Handler : ICommandHandler<CreateUserWithPasswordCommandV1, UserId>
{
    private readonly UserManager<User> _userManager;

    public CreateUserWithPasswordCommandV1Handler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserId> Handle(CreateUserWithPasswordCommandV1 request, CancellationToken cancellationToken)
    {
        var emailAddress = EmailAddress.Create(request.EmailAddress);
        PhoneNumber? phoneNumber = request.PhoneNumber == null ? null : PhoneNumber.Create(request.PhoneNumber);
        var plainPassword = PlainPassword.Create(request.Password);

        if (phoneNumber.HasValue)
        {
            var phoneNumberValue = phoneNumber.Value.Value;
            var isPhoneNumberTaken = await _userManager.Users
                .AnyAsync(user => user.PhoneNumber == phoneNumberValue, cancellationToken);
            if (isPhoneNumberTaken)
            {
                throw new UserPhoneNumberAlreadyInUseException(phoneNumber.Value);
            }
        }

        var userId = UserId.Create();
        var user = new User
        {
            Id = userId.Value,
            UserName = emailAddress.Value,
            Email = emailAddress.Value,
            PhoneNumber = phoneNumber?.Value,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, plainPassword.Value);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                throw new UserEmailAddressAlreadyInUseException(emailAddress);
            }

            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.Member);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        return userId;
    }
}
