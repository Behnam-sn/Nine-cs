using System.Text.RegularExpressions;

using Nine.Identities.Domain.Accounts.Exceptions;

namespace Nine.Identities.Domain.Accounts.ValueObjects;

public readonly partial struct PhoneNumber
{
    [GeneratedRegex(@"^\+?[0-9\s\-\(\)]{7,20}$")]
    private static partial Regex ValidPhoneNumberRegex();
    
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new PhoneNumberCannotBeEmptyException();
        }
        
        value = value.Trim();
        
        if (!ValidPhoneNumberRegex().IsMatch(value))
        {
            throw new PhoneNumberInvalidFormatException();
        }
        
        return new(value);
    }

    
}