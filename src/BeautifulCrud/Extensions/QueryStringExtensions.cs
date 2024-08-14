using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace BeautifulCrud.Extensions;

internal static class QueryStringExtensions
{
    public static IQueryCollection AsQueryCollection(this StringValues queryString)
    {
        var query = QueryHelpers.ParseQuery(queryString);
        var queryCollection = new QueryCollection(query);
        return queryCollection;
    }
}