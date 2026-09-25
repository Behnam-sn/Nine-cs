namespace Nine.Profiles.Infrastructure.Profiles.ReadModels;

public sealed class ProfileHandleLookup
{
    public Guid Id { get; set; }

    public string Handle { get; set; } = string.Empty;
}