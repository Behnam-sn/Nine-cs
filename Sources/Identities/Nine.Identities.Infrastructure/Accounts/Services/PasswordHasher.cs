using System.Security.Cryptography;

using Nine.Identities.Domain.Accounts.Services;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Infrastructure.Accounts.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210_000;
    private const char SegmentSeparator = ':';

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public HashedSecret Hash(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: value,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: Algorithm,
            outputLength: KeySize);

        var hashedSecret = string.Join(
            SegmentSeparator,
            Algorithm.Name,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));

        return HashedSecret.Create(hashedSecret);
    }
}
