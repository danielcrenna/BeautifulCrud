namespace BeautifulCrud.Tests;

public class FilterTests
{
	[Fact]
	public void Basic()
	{
		var list = new List<Foo>
		{
			new() {Bar = "B"},
			new() {Bar = "A"}
		};

		var queryable = list.AsQueryable();
		var query = new ResourceQuery { Filter = "Bar eq 'A'" };

        var options = new CrudOptions();
		var results = queryable.ApplyQuery(query, options).ToList();
		Assert.Single(results);
		Assert.Equal("A", results[0].Bar);
	}

	public sealed class Foo
	{
		public string? Bar { get; set; }
	}
}