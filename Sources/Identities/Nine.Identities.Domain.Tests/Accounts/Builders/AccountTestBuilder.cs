using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Accounts.Enums;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Builders;

internal sealed class AccountTestBuilder
{
    public const string DefaultEmailAddressValue = "john@example.com";
    public const string DefaultPhoneNumberValue = "+123456789";
    public static readonly CredentialId DefaultCredentialId = CredentialId.Create();
    public static readonly CredentialType DefaultCredentialType = CredentialType.Password;
    public static readonly HashedSecret DefaultHashedSecret = HashedSecret.Create("hashedpassword");

    private EmailAddress _emailAddress;
    private PhoneNumber? _phoneNumber;
    private CredentialId _credentialId;
    private CredentialType _credentialType;
    private HashedSecret _hashedSecret;
    private bool _raiseCreationEvent = true;

    public AccountTestBuilder WithEmailAddress(string emailAddress)
    {
        _emailAddress = EmailAddress.Create(emailAddress);
        return this;
    }

    public AccountTestBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = PhoneNumber.Create(phoneNumber);
        return this;
    }

    public AccountTestBuilder AddCredential(CredentialId id, CredentialType type, HashedSecret secret)
    {
        _credentialId = id;
        _credentialType = type;
        _hashedSecret = secret;
        return this;
    }

    public AccountTestBuilder WithoutCreationEvent()
    {
        _raiseCreationEvent = false;
        return this;
    }

    public AccountTestBuilder WithRequiredParameters()
    {
        WithEmailAddress(DefaultEmailAddressValue);
        AddCredential(DefaultCredentialId, DefaultCredentialType, DefaultHashedSecret);
        return this;
    }

    public AccountTestBuilder WithOptionalParameters()
    {
        WithPhoneNumber(DefaultPhoneNumberValue);
        return this;
    }

    public Account Build()
    {
        var account = Account.CreateInstance(
            emailAddress: _emailAddress,
            phoneNumber: _phoneNumber,
            credentialId: _credentialId,
            credentialType: _credentialType,
            hashedSecret: _hashedSecret
        );

        if (!_raiseCreationEvent)
        {
            account.ClearDomainEvents();
        }

        return account;
    }
}
