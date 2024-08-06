namespace BeautifulCrud.Extensions;

internal static class QueryableExtensions
{
    public static bool IsEfCoreQueryable(this IQueryable queryable)
    {
        var provider = queryable.Provider.GetType();
        var isEfCore = provider.Namespace?.StartsWith("Microsoft.EntityFrameworkCore");
        return isEfCore.HasValue && isEfCore.Value;
    }
}