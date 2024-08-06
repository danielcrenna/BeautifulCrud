namespace BeautifulCrud.Tests;

public class SortingTests
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
		var query = new ResourceQuery();

		query.Sort<Foo>("Bar ASC");

        var options = new CrudOptions();
		var results = queryable.ApplyQuery(query, options).ToList();
		Assert.Equal(2, results.Count);
		Assert.Equal("A", results[0].Bar);
		Assert.Equal("B", results[1].Bar);
	}

	public sealed class Foo
	{
		public string? Bar { get; set; }
	}
}