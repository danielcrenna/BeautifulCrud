using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeautifulCrud.AspNetCore.Serialization;

public sealed class OneConverter<T>(JsonNamingPolicy? namingPolicy) : JsonConverter<One<T>>
{
    public override One<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<One<T>>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, One<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(GetPropertyName(nameof(One<T>.Value)));
        JsonSerializer.Serialize(writer, value.Value, options);

        writer.WriteBoolean(GetPropertyName(nameof(One<T>.Found)), value.Found);
        writer.WriteBoolean(GetPropertyName(nameof(One<T>.Error)), value.Error);
        writer.WriteEndObject();
    }

    private string GetPropertyName(string name) => namingPolicy != null ? namingPolicy.ConvertName(name) : name;
}