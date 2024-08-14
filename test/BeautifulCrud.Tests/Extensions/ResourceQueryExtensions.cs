namespace BeautifulCrud.Tests.Extensions;

internal static class ResourceQueryExtensions
{
    public static void AssertValidResourceQuery(this ResourceQuery expected, ResourceQuery actual)
    {
        Assert.NotNull(expected);
        Assert.NotNull(actual);

        Assert.Equal(expected.Projection.Count, actual.Projection.Count);
        _ = expected.Projection.Zip(actual.Projection, (e, a) =>
        {
            Assert.Equal(e.Type, a.Type);
            Assert.Equal(e.Name, a.Name);
            Assert.Equal(e.Next, a.Next);
            return true;
        });

        Assert.Equal(expected.Filter, actual.Filter);

        Assert.Equal(expected.Sorting.Count, actual.Sorting.Count);
        _ = expected.Sorting.Zip(actual.Sorting, (e, a) =>
        {
            Assert.Equal(e.Item1?.Name, a.Item1?.Name);
            Assert.Equal(e.Item1?.PropertyType, a.Item1?.PropertyType);
            Assert.Equal(e.Item2, a.Item2);
            Assert.Equal(e.Item3, a.Item3);
            return true;
        });

        Assert.Equal(expected.Paging != null, actual.Paging != null);
        Assert.Equal(expected.Paging?.MaxPageSize, actual.Paging?.MaxPageSize);
        Assert.Equal(expected.Paging?.PageOffset, actual.Paging?.PageOffset);
        Assert.Equal(expected.Paging?.PageSize, actual.Paging?.PageSize);

        Assert.Equal(expected.Search.Count, actual.Search.Count);
        _ = expected.Search.Zip(actual.Search, (e, a) =>
        {
            Assert.Equal(e.column, a.predicate);
            Assert.Equal(e.column, a.predicate);
            return true;
        });

        Assert.Equal(expected.CountTotalRows, actual.CountTotalRows);
        Assert.Equal(expected.IsDeltaQuery, actual.IsDeltaQuery);
        Assert.Equal(expected.AsOfDateTime, actual.AsOfDateTime);
        Assert.Equal(expected.ServerUri, actual.ServerUri);
    }
}