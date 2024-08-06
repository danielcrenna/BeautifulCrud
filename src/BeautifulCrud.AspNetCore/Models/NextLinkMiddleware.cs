using System.Diagnostics.CodeAnalysis;
using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore.Models;

public class NextLinkMiddleware(RequestDelegate next, 
    IQueryStore queryStore, 
    IEnumerable<EndpointDataSource> endpointDataSources,
    IOptionsMonitor<CrudOptions> options)
{
    private static readonly char[] Slash = ['/'];

    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(HttpContext context)
    {
        var segments = context.Request.Path.Value?.Split(Slash, StringSplitOptions.RemoveEmptyEntries);
        if (segments == null)
        {
            await next(context);
            return;
        }

        TryRedirectToQueryHandler(context, segments);

        await next(context);
    }

    private void TryRedirectToQueryHandler(HttpContext context, string[] segments)
    {
        var nextLinkIndex = Array.FindIndex(segments,
            segment => string.Equals(segment, "nextLink", StringComparison.OrdinalIgnoreCase));

        if (nextLinkIndex == -1 || nextLinkIndex + 1 >= segments.Length) return;

        var continuationToken = segments[nextLinkIndex + 1];
        var endpoints = endpointDataSources.SelectMany(dataSource => dataSource.Endpoints);
        foreach (var endpoint in endpoints)
        {
            var routePattern = (endpoint as RouteEndpoint)?.RoutePattern.RawText;
            if (routePattern == null)
                continue;

            // Minimal API route patterns have a preceding slash, controllers do not
            if(routePattern.StartsWith('/'))
                routePattern = routePattern[1..];

            if (!routePattern.Equals(segments[0], StringComparison.OrdinalIgnoreCase))
                continue;

            if (!ResolveType(endpoint, out var type))
                continue;

            var query = queryStore.GetQueryFromHash(type, continuationToken);
            if (query?.Paging != null)
                query.Paging.PageOffset += query.Paging.PageSize.GetValueOrDefault(options.CurrentValue.DefaultPageSize);

            if (!context.Items.TryAdd(nameof(ResourceQuery), query))
                continue;

            context.Request.Path = $"/{routePattern}";
            context.SetEndpoint(endpoint);
            return;
        }
    }

    private bool ResolveType(Endpoint endpoint, [NotNullWhen(true)] out Type? type)
    {
        type = default;

        if (options.CurrentValue.Features.HasFlagFast(Features.MinimalApis))
            type = endpoint.LookupType();

        if (type == default && options.CurrentValue.Features.HasFlagFast(Features.Controllers))
            type = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.LookupType();

        return type != null;
    }
}

