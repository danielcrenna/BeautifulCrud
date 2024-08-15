using BeautifulCrud;
using BeautifulCrud.AspNetCore.Attributes;
using BeautifulCrud.AspNetCore.Extensions;
using Demo.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Demo.Controllers.Controllers;

// ReSharper disable once SuggestBaseTypeForParameterInConstructor

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IStringLocalizer<WeatherForecastController> localizer, IOptionsSnapshot<CrudOptions> options) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static readonly List<WeatherForecast> Data = CreateData();

    [HttpGet(Name = "GetWeatherForecast")]
    [CollectionQuery<WeatherForecast>, Prefer]
    public async Task<IActionResult> Get(ResourceQuery query, CancellationToken cancellationToken)
    {
        var data = Data.AsQueryable();
        var result = await data.ToManyAsync(query, options.Value, cancellationToken);

        if (query.PreferMinimal)
        {
            return result.Value == null
                ? this.NotFoundWithProblemDetails(localizer,
                    "Prefer: return=minimal was requested, but no results match the query.")
                : NoContent();
        }

        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetWeatherForecastById")]
    [ItemQuery<WeatherForecast>, Prefer]
    public async Task<IActionResult> GetById(Guid id, ResourceQuery query, CancellationToken cancellationToken)
    {
        var data = Data.AsQueryable();
        var result = await data.ToOneAsync(query, id, cancellationToken);

        if (!result.Found)
            return this.NotFoundWithProblemDetails(localizer,
                $"The resource with ID {id} was not found");

        if (query.PreferMinimal)
            return NoContent();

        return Ok(result);
    }

    private static List<WeatherForecast> CreateData()
    {
        return Enumerable.Range(1, 50).Select(index => new WeatherForecast
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();
    }
}