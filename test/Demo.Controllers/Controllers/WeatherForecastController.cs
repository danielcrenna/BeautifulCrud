using BeautifulCrud;
using BeautifulCrud.AspNetCore.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Demo.Controllers.Controllers;

// ReSharper disable once SuggestBaseTypeForParameterInConstructor

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IOptionsSnapshot<CrudOptions> options) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static readonly List<WeatherForecast> Data = CreateData();
    
    [HttpGet(Name = "GetWeatherForecast")]
	[CollectionQuery, Prefer]
    public async Task<Many<WeatherForecast>> Get(ResourceQuery query, CancellationToken cancellationToken)
    {
	    var data = Data.AsQueryable();
	    var result = await data.ToManyAsync(query, options.Value, cancellationToken);
        return result;
    }

    [HttpGet("{id:guid}", Name = "GetWeatherForecastById")]
    [ItemQuery, Prefer]
    public async Task<One<WeatherForecast>> GetById(Guid id, ResourceQuery query, CancellationToken cancellationToken)
    {
        var data = Data.AsQueryable();
        var result = await data.ToOneAsync(query, id, cancellationToken);
        return result;
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