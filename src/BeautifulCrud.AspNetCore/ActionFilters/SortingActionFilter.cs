using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore.ActionFilters;

public sealed class SortingActionFilter(IOptionsMonitor<CrudOptions> options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.HttpContext.ApplySorting(context.ResolveType<SortingActionFilter>(), options.CurrentValue);
        await next.Invoke();
    }
}