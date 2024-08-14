using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
namespace BeautifulCrud.AspNetCore.ActionFilters;

public sealed class CollectionQueryActionFilter(IOptionsMonitor<CrudOptions> options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var type = context.ResolveType<CollectionQueryActionFilter>();
        if (type == null)
            return;

        var query = context.GetResourceQuery();
        query.ServerUri = context.HttpContext.Request.GetServerUri();
        query.Parse(type, context.HttpContext.Request.Query, options.CurrentValue);
        
        await context.ShapeSelect(next);
    }
}