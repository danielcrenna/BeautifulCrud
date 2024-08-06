using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeautifulCrud.AspNetCore.Extensions;

internal static class ResourceQueryExtensions
{
    public static ResourceQuery GetResourceQuery(this EndpointFilterInvocationContext context) => context.HttpContext.GetResourceQuery();
    public static ResourceQuery GetResourceQuery(this ActionExecutingContext context) => context.HttpContext.GetResourceQuery();
}