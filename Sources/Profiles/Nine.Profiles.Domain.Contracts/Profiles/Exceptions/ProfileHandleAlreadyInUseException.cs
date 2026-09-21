using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Contracts.Profiles.Exceptions;

public class ProfileHandleAlreadyInUseException(ProfileHandle handle)
    : Exception($"The {handle.Value} Profile Handle Already In Use");
