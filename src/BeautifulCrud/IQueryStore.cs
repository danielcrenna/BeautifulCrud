namespace BeautifulCrud;

public interface IQueryStore
{
    string? BuildQueryHash(Type context, ResourceQuery query);
    ResourceQuery? GetQueryFromHash(Type context, string? queryHash);
}