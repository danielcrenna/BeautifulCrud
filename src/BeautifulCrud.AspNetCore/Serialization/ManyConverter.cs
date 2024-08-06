using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeautifulCrud.AspNetCore.Serialization;

public sealed class ManyConverter<T>(JsonNamingPolicy? namingPolicy) : JsonConverter<Many<T>>
{
    public override Many<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<Many<T>>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, Many<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(GetPropertyName(nameof(Many<T>.Value)));
        JsonSerializer.Serialize(writer, value.Value, options);

        writer.WriteNumber(GetPropertyName(nameof(Many<T>.Items)), value.Items);

        if (!string.IsNullOrWhiteSpace(value.NextLink))
            writer.WriteString("@nextLink", value.NextLink);

        if (!string.IsNullOrWhiteSpace(value.DeltaLink))
            writer.WriteString("@deltaLink", value.DeltaLink);

        writer.WriteEndObject();
    }

    private string GetPropertyName(string name) => namingPolicy != null ? namingPolicy.ConvertName(name) : name;
}