using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Accounts.Repositories;
using Nine.Identities.Domain.Accounts.Services;
using Nine.Identities.Domain.Contracts.Accounts.Exceptions;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.Identities.Application.Accounts.Commands.CreateAccountWithPassword;

public sealed class CreateAccountWithPasswordCommandV1Handler : ICommandHandler<CreateAccountWithPasswordCommandV1, AccountId>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountEmailAddressUniquenessChecker _accountEmailAddressUniquenessChecker;
    private readonly IAccountPhoneNumberUniquenessChecker _accountPhoneNumberUniquenessChecker;
    private readonly IPasswordHasher _passwordHasher;

    public CreateAccountWithPasswordCommandV1Handler(
        IAccountRepository accountRepository,
        IAccountEmailAddressUniquenessChecker accountEmailAddressUniquenessChecker,
        IAccountPhoneNumberUniquenessChecker accountPhoneNumberUniquenessChecker,
        IPasswordHasher passwordHasher
    )
    {
        _accountRepository = accountRepository;
        _accountEmailAddressUniquenessChecker = accountEmailAddressUniquenessChecker;
        _accountPhoneNumberUniquenessChecker = accountPhoneNumberUniquenessChecker;
        _passwordHasher = passwordHasher;
    }

    public async Task<AccountId> Handle(CreateAccountWithPasswordCommandV1 request, CancellationToken cancellationToken)
    {
        var emailAddress = EmailAddress.Create(request.EmailAddress);

        var isEmailAddressTaken = await _accountEmailAddressUniquenessChecker.IsTakenAsync(emailAddress, cancellationToken);
        if (isEmailAddressTaken)
        {
            throw new AccountEmailAddressAlreadyInUseException(emailAddress);
        }

        PhoneNumber? phoneNumber = request.PhoneNumber == null ? null : PhoneNumber.Create(request.PhoneNumber);

        if (phoneNumber.HasValue)
        {
            var isPhoneNumberTaken = await _accountPhoneNumberUniquenessChecker.IsTakenAsync(phoneNumber.Value, cancellationToken);
            if (isPhoneNumberTaken)
            {
                throw new AccountPhoneNumberAlreadyInUseException(phoneNumber.Value);
            }
        }

        var plainPassword = PlainPassword.Create(request.Password);
        var hashedPassword = _passwordHasher.Hash(plainPassword.Value);

        var account = Account.CreateWithPassword(
            emailAddress: emailAddress,
            phoneNumber: phoneNumber,
            hashedPassword: hashedPassword
        );

        await _accountRepository.AddAsync(account, cancellationToken);

        return account.AccountId;
    }
}
