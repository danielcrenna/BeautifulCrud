using BeautifulCrud.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BeautifulCrud.AspNetCore.EndpointFilters;

public sealed class PagingEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptionsMonitor<CrudOptions>>();
	    context.HttpContext.ApplyPaging(options.CurrentValue);
	    return await next.Invoke(context);
    }
}