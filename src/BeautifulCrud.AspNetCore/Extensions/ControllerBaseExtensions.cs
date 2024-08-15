using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BeautifulCrud.AspNetCore.Extensions;

public static class ControllerBaseExtensions
{
    public static IActionResult NotFoundWithDetails(this ControllerBase controller, IStringLocalizer localizer, string details, params object[] arguments)
    {
        return controller.StatusCodeWithProblemDetails(localizer, StatusCodes.Status404NotFound, localizer.GetString("Not Found"), details, arguments);
    }

    public static IActionResult GoneWithDetails(this ControllerBase controller, IStringLocalizer localizer, string details, params object[] arguments)
    {
        return controller.StatusCodeWithProblemDetails(localizer, StatusCodes.Status410Gone, localizer.GetString("Gone"), details, arguments);
    }

    public static IActionResult StatusCodeWithProblemDetails(this ControllerBase controller, IStringLocalizer localizer, int statusCode, string statusDescription, string details, params object[] arguments)
    {
        var section = statusCode switch
        {
            400 => "6.5.1",
            404 => "6.5.4",
            410 => "6.5.9",
            _ => throw new NotImplementedException($"Missing section for status code {statusCode}")
        };

        var model = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://tools.ietf.org/html/rfc7231#section-{section}",
            Title = localizer.GetString(statusDescription),
            Detail = localizer.GetString(details, arguments),
            Instance = controller.Request.GetEncodedPathAndQuery()
        };
        model.Extensions.Add("TraceId", Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier);
        return controller.StatusCode(statusCode, model);
    }
}