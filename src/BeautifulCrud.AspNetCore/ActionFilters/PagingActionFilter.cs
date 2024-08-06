using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore.ActionFilters;

public sealed class PagingActionFilter(IOptionsMonitor<CrudOptions> options) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
	    context.HttpContext.ApplyPaging(options.CurrentValue);
        await next.Invoke();
    }
}