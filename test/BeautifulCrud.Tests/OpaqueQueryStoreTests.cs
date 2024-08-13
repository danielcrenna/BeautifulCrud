namespace BeautifulCrud.Tests;

public class OpaqueQueryStoreTests
{
    [Fact]
    public void RoundTripTest()
    {
        var query = new ResourceQuery();
        // query.Sort<WeatherForecast>("Date ASC");
        // query.Project(typeof(WeatherForecast), include: default, select: "Date", exclude: default);

        var now = DateTimeOffset.Now;
        var queryStore = new OpaqueQueryStore(StableTimestamp);
        var type = typeof(WeatherForecast);
        
        var firstHash = queryStore.BuildQueryHash(type, query);
        Assert.NotNull(firstHash);
        Assert.NotEmpty(firstHash);

        var firstHashRestored = queryStore.GetQueryFromHash(type, firstHash);
        Assert.NotNull(firstHashRestored);

        var secondHash = queryStore.BuildQueryHash(type, firstHashRestored);
        Assert.NotNull(secondHash);
        Assert.NotEmpty(secondHash);

        Assert.Equal(firstHash, secondHash);
        return;

        DateTimeOffset StableTimestamp() => now;
    }

    public sealed class WeatherForecast
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string? Summary { get; set; }
    }
}