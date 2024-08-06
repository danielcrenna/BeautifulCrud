using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeautifulCrud.AspNetCore.Serialization;

internal sealed class ExpandoObjectConverter(JsonNamingPolicy? namingPolicy) : JsonConverter<ExpandoObject>
{
	public override ExpandoObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

	public override void Write(Utf8JsonWriter writer, ExpandoObject value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		foreach (var kvp in value)
		{
			var propertyName = namingPolicy != null ? namingPolicy.ConvertName(kvp.Key) : kvp.Key;
			writer.WritePropertyName(propertyName);
			JsonSerializer.Serialize(writer, kvp.Value, kvp.Value?.GetType() ?? typeof(object), options);
		}

		writer.WriteEndObject();
	}
}