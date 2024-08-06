using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore.ActionFilters;

public sealed class ProjectActionFilter(IOptionsMonitor<CrudOptions> options) : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    { 
        context.HttpContext.ApplyProjection(context.ResolveType<ProjectActionFilter>(), options.CurrentValue);
        await context.ShapeSelect(next);
    }
}