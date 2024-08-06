using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeautifulCrud.AspNetCore.Extensions;

internal static class ShapingExtensions
{
    public static async Task ShapeSelect(this ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next.Invoke();
        var query = context.GetResourceQuery();
        if(query.Projection.Count > 0 && executed.Result is ObjectResult { Value: not null } objectResult)
            objectResult.Value = query.Project(objectResult.Value);
    }

    public static async Task<object?> ShapeSelect(this EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var executed = await next.Invoke(context);
        var query = context.GetResourceQuery();
        if(query.Projection.Count > 0 && executed != null)
            executed = query.Project(executed);
        return executed;
    }

    public static async Task ShapePrefer(this ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next.Invoke();
        if (TryAddPreferenceApplied(context.HttpContext))
            executed.Result = null;
    }

    public static async Task<object?> ShapePrefer(this EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var executed = await next.Invoke(context);
        if (TryAddPreferenceApplied(context.HttpContext))
            executed = null;
        return executed;
    }

    private static bool TryAddPreferenceApplied(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Prefer", out var prefer))
            return false;

        foreach (var _ in prefer.Where(x =>
                     x != null && x.Equals("return=minimal", StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Headers.TryAdd("Preference-Applied", "return=minimal");
            return true;
        }

        foreach (var _ in prefer.Where(x =>
                     x != null && x.Equals("return=representation", StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Headers.TryAdd("Preference-Applied", "return=representation");
            return false;
        }

        return false;
    }
}