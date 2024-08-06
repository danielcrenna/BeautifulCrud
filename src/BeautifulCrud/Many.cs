using System.Text.Json.Serialization;
using BeautifulCrud.Extensions;

namespace BeautifulCrud;

public class Many<T>
{
	private static readonly IEnumerable<T> EmptyEnumerable = Enumerable.Empty<T>();
	private static readonly List<T> EmptyList = EmptyEnumerable.ToList();

	public Many()
	{
		Value = EmptyList;
	}

	public Many(IEnumerable<T>? value, string? nextLink, string? deltaLink)
	{
		Value = value?.AsList();
		NextLink = nextLink;
		DeltaLink = deltaLink;
	}

	public List<T>? Value { get; set; }
	public int Items => Value?.Count ?? 0;
	
    [JsonPropertyName("@nextLink")]
    public string? NextLink { get; set; }

    [JsonPropertyName("@deltaLink")]
    public string? DeltaLink { get; set; }
}