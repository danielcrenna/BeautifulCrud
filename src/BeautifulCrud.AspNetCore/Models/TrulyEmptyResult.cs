using Microsoft.AspNetCore.Http;

namespace BeautifulCrud.AspNetCore.Models;

public class TrulyEmptyResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        return Task.CompletedTask;
    }
}