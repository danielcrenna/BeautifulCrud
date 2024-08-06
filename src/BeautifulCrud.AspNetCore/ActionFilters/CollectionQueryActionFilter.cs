using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore.ActionFilters;

public sealed class CollectionQueryActionFilter(IOptionsMonitor<CrudOptions> options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var type = context.ResolveType<CollectionQueryActionFilter>();
        
        context.HttpContext.ApplyProjection(type, options.CurrentValue);
        context.HttpContext.ApplyFilter(options.CurrentValue);
        context.HttpContext.ApplySorting(type, options.CurrentValue);
        context.HttpContext.ApplyPaging(options.CurrentValue);
        context.HttpContext.ApplyCount(options.CurrentValue);
        
        await context.ShapeSelect(next);
    }
}