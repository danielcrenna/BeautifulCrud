using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BeautifulCrud.AspNetCore.Extensions;

public static class ControllerBaseExtensions
{
    public static IActionResult NotFoundWithProblemDetails(this ControllerBase controller, IStringLocalizer localizer, string details, params object[] arguments)
    {
        return controller.StatusCode(StatusCodes.Status404NotFound, controller.HttpContext.NotFoundWithProblemDetails(localizer, details, arguments));
    }

    public static IActionResult GoneWithProblemDetails(this ControllerBase controller, IStringLocalizer localizer, string details, params object[] arguments)
    {
        return controller.StatusCode(StatusCodes.Status410Gone, controller.HttpContext.GoneWithProblemDetails(localizer, details, arguments));
    }

    public static IActionResult StatusCodeWithProblemDetails(this ControllerBase controller, IStringLocalizer localizer, int statusCode, string statusDescription, string details, params object[] arguments)
    {
        return controller.StatusCode(statusCode, controller.HttpContext.StatusCodeWithProblemDetails(localizer, statusCode, statusDescription, details, arguments));
    }
}