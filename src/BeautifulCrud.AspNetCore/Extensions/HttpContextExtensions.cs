using Microsoft.AspNetCore.Http;

namespace BeautifulCrud.AspNetCore.Extensions;

internal static class HttpContextExtensions
{
    public static void ApplyProjection(this HttpContext context, Type? type, CrudOptions options)
    {
        if (type == null)
            return;

        var query = context.GetResourceQuery();
        query.ApplyProjection(type, context.Request.Query, options);
    }

    public static void ApplyFilter(this HttpContext context, CrudOptions options)
    {
        var query = context.GetResourceQuery();
        query.ApplyFilter(context.Request.Query, options);
    }

    public static void ApplySorting(this HttpContext context, Type? type, CrudOptions options)
    {
        if (type == null)
            return;

        var query = context.GetResourceQuery();
        query.ApplySorting(type, context.Request.Query, options);
    }

    public static void ApplyPaging(this HttpContext context, CrudOptions options)
    {
        var query = context.GetResourceQuery();
        query.ServerUri = context.Request.GetServerUri();
        query.ApplyPaging(context.Request.Query, options);
    }

    public static void ApplyCount(this HttpContext context, CrudOptions options)
    {
        var query = context.GetResourceQuery();
        query.ApplyCount(context.Request.Query, options);
    }

    public static void ApplyPrefer(this HttpContext context)
    {
        var query = context.GetResourceQuery();

        query.PreferMinimal = context.Request.Headers.TryGetValue("Prefer", out var preferHeader) && preferHeader
            .Contains("return=minimal", StringComparer.OrdinalIgnoreCase);
    }
}