using BeautifulCrud.AspNetCore.Extensions;
using BeautifulCrud.AspNetCore.Models;
using Microsoft.AspNetCore.Http;

namespace BeautifulCrud.AspNetCore.EndpointFilters;

public sealed class PreferEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.ApplyPrefer();
        var result = await context.ShapePrefer(next);
        return result ?? new TrulyEmptyResult();
    }
}