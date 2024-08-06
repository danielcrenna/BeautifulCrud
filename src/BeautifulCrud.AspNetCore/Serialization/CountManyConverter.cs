using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeautifulCrud.AspNetCore.Serialization;

public sealed class CountManyConverter<T>(JsonNamingPolicy? namingPolicy) : JsonConverter<CountMany<T>>
{
    public override CountMany<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<CountMany<T>>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, CountMany<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(GetPropertyName(nameof(CountMany<T>.Value)));
        JsonSerializer.Serialize(writer, value.Value, options);

        writer.WriteNumber(GetPropertyName(nameof(CountMany<T>.Items)), value.Items);
        writer.WriteNumber(GetPropertyName(nameof(CountMany<T>.MaxItems)), value.MaxItems.GetValueOrDefault());

        if (!string.IsNullOrWhiteSpace(value.NextLink))
            writer.WriteString("@nextLink", value.NextLink);

        if (!string.IsNullOrWhiteSpace(value.DeltaLink))
            writer.WriteString("@deltaLink", value.DeltaLink);

        writer.WriteEndObject();
    }

    private string GetPropertyName(string name) => namingPolicy != null ? namingPolicy.ConvertName(name) : name;
}