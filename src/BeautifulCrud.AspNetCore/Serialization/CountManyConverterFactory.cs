using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeautifulCrud.AspNetCore.Serialization;

public sealed class CountManyConverterFactory(JsonNamingPolicy? namingPolicy) : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(CountMany<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgument = typeToConvert.GetGenericArguments()[0];
        var converter = (JsonConverter)Activator.CreateInstance(
            typeof(CountManyConverter<>).MakeGenericType(typeArgument),
            [namingPolicy])!;
        return converter;
    }
}