using BeautifulCrud.Extensions;

namespace BeautifulCrud;

public class CountMany<T> : Many<T>
{
	public CountMany()
	{
		Value = Enumerable.Empty<T>().ToList();
		MaxItems = 0;
	}

	public CountMany(IEnumerable<T>? value, int maxItems, string? nextLink, string? deltaLink)
	{
		Value = value?.AsList();
		MaxItems = maxItems;
		NextLink = nextLink;
		DeltaLink = deltaLink;
	}

	public int? MaxItems { get; set; }
}