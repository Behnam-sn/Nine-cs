namespace Nine.Domain.Users.ValueObjects;

public struct FirstName
{
    public string Value { get; }

    public FirstName(string value)
    {
        Value = value;
    }
    
    public static FirstName Create(string value)
    {
        return new FirstName(value);
    } 
}