using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeautifulCrud.AspNetCore.Serialization;

public sealed class OneConverterFactory(JsonNamingPolicy? namingPolicy) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(One<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgument = typeToConvert.GetGenericArguments()[0];
        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(OneConverter<>).MakeGenericType(typeArgument),
            [namingPolicy])!;
        return converter;
    }
}