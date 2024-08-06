using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BeautifulCrud.AspNetCore.ActionFilters;

public sealed class PreferActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.HttpContext.ApplyPrefer();
        await context.ShapePrefer(next);
    }
}