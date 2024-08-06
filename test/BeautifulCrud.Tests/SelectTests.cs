namespace BeautifulCrud.Tests;

public class SelectTests
{
	[Fact]
	public void TopLevelProjection()
	{
		var list = new List<Three>
		{
			new() {Four = "4", Five = "5"}
		};

		var queryable = list.AsQueryable();
		var query = new ResourceQuery();

		query.Project(typeof(Three), default, select: "Four", default);

        var options = new CrudOptions();
		var results = queryable.ApplyQuery(query, options).ToList();

		Assert.Single(results);
		Assert.Equal("4", results[0].Four);
		Assert.Null(results[0].Five);
	}
     
    [Fact]
    public void OneLevelProjection()
    {
        var list = new List<Two>
        {
			new() { Three = new Three { Four = "4", Five = "5" }}
        };

        var queryable = list.AsQueryable();
        var query = new ResourceQuery();

		query.Project(typeof(Two), default, select: "Three.Four" , default);

        var options = new CrudOptions();
        var results = queryable.ApplyQuery(query, options).ToList();

        Assert.Single(results);
        Assert.Equal("4", results[0].Three?.Four);
        Assert.Null(results[0].Three?.Five);
    }

    [Fact]
    public void TwoLevelProjection()
    {
        var list = new List<One>
        {
            new()
            {
                Two = new Two
                {
                    Three = new Three
                    {
                        Four = "4", 
                        Five = "5"
                    }
                }
            }
        };

        var queryable = list.AsQueryable();
        var query = new ResourceQuery();
        
        query.Project(typeof(One), default, select: "Two.Three.Four" , default);

        var options = new CrudOptions();
        var results = queryable.ApplyQuery(query, options).ToList();

        Assert.Single(results);
        Assert.Equal("4", results[0].Two?.Three?.Four);
        Assert.Null(results[0].Two?.Three?.Five);
    }

    public sealed class One
    {
        public Two? Two { get; set; }
    }

    public sealed class Two
    {
        public Three? Three { get; set; }
    }

    public sealed class Three
    {
        public string? Four { get; set; }
        public string? Five { get; set; }
    }
}