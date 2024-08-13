using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace BeautifulCrud.Extensions;

internal static class QueryStringExtensions
{
    public static IQueryCollection AsQueryCollection(this string queryString)
    {
        var query = QueryHelpers.ParseQuery(queryString);
        var queryCollection = new QueryCollection(query);
        return queryCollection;
    }
}