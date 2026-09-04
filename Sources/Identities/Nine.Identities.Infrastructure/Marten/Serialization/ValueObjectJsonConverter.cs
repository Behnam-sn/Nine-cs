using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nine.Identities.Infrastructure.Marten.Serialization;

internal sealed class ValueObjectJsonConverter<TValueObject, TValue> : JsonConverter<TValueObject>
    where TValueObject : struct
    where TValue : notnull
{
    private readonly Func<TValueObject, TValue> _getter;
    private readonly Func<TValue, TValueObject> _factory;

    public ValueObjectJsonConverter(Func<TValueObject, TValue> getter, Func<TValue, TValueObject> factory)
    {
        _getter = getter;
        _factory = factory;
    }

    public override TValueObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
        if (value is null)
        {
            throw new JsonException($"Cannot deserialize {typeof(TValueObject).Name} from null.");
        }

        return _factory(value);
    }

    public override void Write(Utf8JsonWriter writer, TValueObject value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, _getter(value), options);
    }
}
