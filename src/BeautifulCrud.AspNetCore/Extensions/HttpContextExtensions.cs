using Microsoft.AspNetCore.Http;

namespace BeautifulCrud.AspNetCore.Extensions;

internal static class HttpContextExtensions
{
    public static void ApplyProjection(this HttpContext context, Type? type, CrudOptions options)
    {
        if (type == null)
            return;

        var query = context.GetResourceQuery();

        query.Project(type, context.Request.Query, options);
    }

    public static void ApplyFilter(this HttpContext context, CrudOptions options)
    {
        var query = context.GetResourceQuery();

        if (!context.Request.Query.TryGetValue(options.FilterOperator, out var clauses) || clauses.Count == 0)
            return;

        query.Filter = clauses;
    }

    public static void ApplySorting(this HttpContext context, Type? type, CrudOptions options)
    {
        if (type == null)
            return;

        var query = context.GetResourceQuery();
        query.Sorting.Clear();

        if (context.Request.Query.TryGetValue(options.OrderByOperator, out var clauses))
            query.Sort(type, clauses);

        query.ApplyDefaultSort(type);
    }

    public static void ApplyPaging(this HttpContext context, CrudOptions options)
    {
        var query = context.GetResourceQuery();
        query.ServerUri = context.Request.GetServerUri();

        context.Request.Query.TryGetValue(options.MaxPageSizeOperator, out var maxPageSize);
        context.Request.Query.TryGetValue(options.SkipOperator, out var skip);
        context.Request.Query.TryGetValue(options.TopOperator, out var top);

        query.Paging(maxPageSize, skip, top, options);
    }

    public static void ApplyCount(this HttpContext context, CrudOptions options)
    {
        var query = context.GetResourceQuery();

        if (!context.Request.Query.TryGetValue(options.CountOperator, out var clauses))
            return;

        if (clauses.Count <= 0)
            return;

        foreach (var clause in clauses.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (clause != null && (clause.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                   int.TryParse(clause, out var countAsNumber) && countAsNumber == 1 ||
                                   clause.Equals("yes", StringComparison.OrdinalIgnoreCase)))

                query.CountTotalRows = true;
        }
    }

    public static void ApplyPrefer(this HttpContext context)
    {
        var query = context.GetResourceQuery();

        query.PreferMinimal = context.Request.Headers.TryGetValue("Prefer", out var preferHeader) && preferHeader
            .Contains("return=minimal", StringComparer.OrdinalIgnoreCase);
    }
}