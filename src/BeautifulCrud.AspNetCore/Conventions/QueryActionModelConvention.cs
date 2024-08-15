using System.Net.Mime;
using BeautifulCrud.AspNetCore.ActionFilters;
using BeautifulCrud.AspNetCore.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BeautifulCrud.AspNetCore.Conventions;

public class QueryActionModelConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        TryAnnotateCollectionQueries(action);
        TryAnnotateItemQueries(action);
    }

    private static void TryAnnotateCollectionQueries(ActionModel action)
    {
        var collectionQueryType = action.ResolveType<CollectionQueryActionFilter>();
        if (collectionQueryType == null) return;

        action.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json, []));
        action.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json, []));
        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(Many<>).MakeGenericType(collectionQueryType), StatusCodes.Status200OK));
        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(CountMany<>).MakeGenericType(collectionQueryType), StatusCodes.Status200OK));

        var preferQueryType = action.ResolveType<PreferActionFilter>();
        if (preferQueryType == null) return;

        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));
        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status404NotFound));
    }
    
    private static void TryAnnotateItemQueries(ActionModel action)
    {
        var itemQueryType = action.ResolveType<ItemQueryAttribute>();
        if (itemQueryType == null) return;

        action.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json, []));
        action.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json, []));
        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(One<>).MakeGenericType(itemQueryType), StatusCodes.Status200OK));
        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status404NotFound));

        var preferQueryType = action.ResolveType<PreferActionFilter>();
        if (preferQueryType == null) return;

        action.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status204NoContent));
    }
}