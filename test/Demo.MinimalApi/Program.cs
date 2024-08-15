using BeautifulCrud;
using BeautifulCrud.AspNetCore;
using BeautifulCrud.AspNetCore.Extensions;
using Demo.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBeautifulCrud(o => { o.Features = Features.MinimalApis | Features.OpenApi; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseBeautifulCrud();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var data = Enumerable.Range(1, 50).Select(index => new WeatherForecast
{
    Id = Guid.NewGuid(),
    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    TemperatureC = Random.Shared.Next(-20, 55),
    Summary = summaries[Random.Shared.Next(summaries.Length)]
}).ToList();

app.MapGet("/weatherforecast",
        async (HttpContext context, ResourceQuery query, [FromServices] IStringLocalizer<WeatherForecast> localizer, IOptionsSnapshot<CrudOptions> options, CancellationToken cancellationToken) =>
        {
            var result = await data
                .AsQueryable()
                .ToManyAsync(query, options.Value, cancellationToken);

            if (!query.PreferMinimal)
                return Results.Ok(result);

            if (result.Value != null)
                return Results.NoContent();

            return Results.Problem(context.NotFoundWithProblemDetails(localizer,
                "Prefer: return=minimal was requested, but no results match the query."));
        })
    .CollectionQuery<WeatherForecast>()
    .Prefer()
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/weatherforecast/{id:guid}",
        async (HttpContext context, Guid id, ResourceQuery query, [FromServices] IStringLocalizer<WeatherForecast> localizer, CancellationToken cancellationToken) =>
        {
            var result = await data.AsQueryable().ToOneAsync(query, id, cancellationToken);

            if (!result.Found)
            {
                return Results.Problem(context.NotFoundWithProblemDetails(localizer,
                    $"The resource with ID {id} was not found"));
            }

            if (!query.PreferMinimal)
                return Results.Ok(result);
            
            return Results.NoContent();
        })
    .ItemQuery<WeatherForecast>()
    .Prefer()
    .WithName("GetWeatherForecastById")
    .WithOpenApi();

app.Run();