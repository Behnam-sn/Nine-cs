using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Nine.Identities.Domain.Contracts.Users.Exceptions;
using Nine.Identities.Domain.Contracts.Users.ValueObjects;
using Nine.Identities.Domain.Users.Entities;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Application.Users.Commands.CreateUserWithPassword;

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
        if (result.Succeeded)
        {
            return userId;
        }

        if (result.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            throw new UserEmailAddressAlreadyInUseException(emailAddress);
        }

        throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
