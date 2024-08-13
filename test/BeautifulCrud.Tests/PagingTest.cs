namespace BeautifulCrud.Tests;

public class PagingTest
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

        var options = new CrudOptions();
        query.Paging("$skip=1&$top=1", options);

		var results = queryable.ApplyQuery(query, options).ToList();
		Assert.Single(results);
		Assert.Equal("A", results[0].Bar);
	}

	public sealed class Foo
	{
		public string? Bar { get; set; }
	}
}