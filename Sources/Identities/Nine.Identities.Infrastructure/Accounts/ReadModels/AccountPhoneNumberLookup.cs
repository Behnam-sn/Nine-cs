namespace Nine.Identities.Infrastructure.Accounts.ReadModels;

public sealed class AccountPhoneNumberLookup
{
    public Guid Id { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
}
