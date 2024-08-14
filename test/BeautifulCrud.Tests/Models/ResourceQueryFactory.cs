namespace BeautifulCrud.Tests.Models;

public static class ResourceQueryFactory
{
    public static ResourceQuery BuildResourceQuery(DateTimeOffset timestamp)
    {
        var options = new CrudOptions();

        var expected = new ResourceQuery();
        expected.Project<WeatherForecast>("$select=Date", options);
        expected.Filter = "TemperatureC eq 100";
        expected.Sort<WeatherForecast>("Date DESC");
        expected.Paging("$skip=1&$top=1", options);
        expected.Search.Add(new ValueTuple<string, string>("Date", "< today()"));
        expected.CountTotalRows = true;
        expected.IsDeltaQuery = true;
        expected.AsOfDateTime = timestamp;
        expected.ServerUri = new Uri("https://localhost:5000/", UriKind.Absolute);

        return expected;
    }
}