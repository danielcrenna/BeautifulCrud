using BeautifulCrud.AspNetCore.Extensions;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore;

public static class Magic
{
    public static IEndpointRouteBuilder MapBeautifulCrud<T, TKey>(this IEndpointRouteBuilder builder, IQueryable<T> queryable) where T : class, IKeyed<TKey>, new()
    {
        builder
            .MapGet(queryable)
            .MapGetById<T, TKey>(queryable);

        return builder;
    }

    public static IEndpointRouteBuilder MapGet<T>(this IEndpointRouteBuilder builder, IQueryable<T> queryable, string? pattern = default, string? endpointName = default) where T : class
    {
        pattern ??= $"/{typeof(T).Name.Pluralize().ToLowerInvariant()}";
        endpointName ??= $"Get{typeof(T).Name.Pluralize()}";

        builder.MapGet(pattern,
                async (HttpContext context, ResourceQuery query, [FromServices] IStringLocalizer<T> localizer,
                    IOptionsSnapshot<CrudOptions> options, CancellationToken cancellationToken) =>
                {
                    var result = await queryable
                        .ToManyAsync(query, options.Value, cancellationToken);

                    if (!query.PreferMinimal)
                        return Results.Ok(result);

                    if (result.Value != null)
                        return Results.NoContent();

                    return Results.Problem(context.NotFoundWithProblemDetails(localizer,
                        "Prefer: return=minimal was requested, but no results match the query."));
                })
            .CollectionQuery<T>()
            .Prefer()
            .WithName(endpointName)
            .WithOpenApi()
            ;

        return builder;
    }

    public static RouteHandlerBuilder MapGetById<T, TKey>(this IEndpointRouteBuilder builder, IQueryable<T> queryable, string? pattern = default, string? endpointName = default) 
        where T : class, IKeyed<TKey>, new()
    {
        pattern ??= $"/{typeof(T).Name.Pluralize().ToLowerInvariant()}/{{id}}";
        endpointName ??= $"Get{typeof(T).Name.Singularize()}ById";

        var map = builder.MapGet(pattern,
                async (HttpContext context, TKey id, ResourceQuery query, [FromServices] IStringLocalizer<T> localizer, CancellationToken cancellationToken) =>
                {
                    var result = await queryable.ToOneAsync(query, id, cancellationToken);

                    if (!result.Found)
                    {
                        return Results.Problem(context.NotFoundWithProblemDetails(localizer,
                            $"The resource with ID {id} was not found"));
                    }

                    if (!query.PreferMinimal)
                        return Results.Ok(result);
            
                    return Results.NoContent();
                })
            .ItemQuery<T>()
            .Prefer()
            .WithName(endpointName)
            .WithOpenApi();

        return map;
    }
}