using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace BeautifulCrud.AspNetCore.Extensions;

public static class HttpContextExtensions
{
    public static ProblemDetails NotFoundWithProblemDetails(this HttpContext context, IStringLocalizer localizer, string details, params object[] arguments)
    {
        return context.StatusCodeWithProblemDetails(localizer, StatusCodes.Status404NotFound, localizer.GetString("Not Found"), details, arguments);
    }

    public static ProblemDetails GoneWithProblemDetails(this HttpContext context, IStringLocalizer localizer, string details, params object[] arguments)
    {
        return context.StatusCodeWithProblemDetails(localizer, StatusCodes.Status410Gone, localizer.GetString("Gone"), details, arguments);
    }

    public static ProblemDetails StatusCodeWithProblemDetails(this HttpContext context, IStringLocalizer localizer, int statusCode, string statusDescription, string details, params object[] arguments)
    {
        var section = statusCode switch
        {
            400 => "6.5.1",
            404 => "6.5.4",
            410 => "6.5.9",
            _ => throw new NotImplementedException($"Missing section for status code {statusCode}")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://tools.ietf.org/html/rfc7231#section-{section}",
            Title = localizer.GetString(statusDescription),
            Detail = localizer.GetString(details, arguments),
            Instance = context.Request.GetEncodedPathAndQuery()
        };

        problemDetails.Extensions.Add("TraceId", Activity.Current?.Id ?? context.TraceIdentifier);
        
        return problemDetails;
    }

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