namespace Nine.Identities.Infrastructure.Accounts.ReadModels;

public sealed class AccountEmailAddressLookup
{
    public Guid Id { get; set; }

    public string EmailAddress { get; set; } = string.Empty;
}
