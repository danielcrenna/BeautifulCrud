using BeautifulCrud;
using BeautifulCrud.AspNetCore;
using Demo.Shared;
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
        async (ResourceQuery query, IOptionsSnapshot<CrudOptions> options, CancellationToken cancellationToken) =>
            await data.AsQueryable().ToManyAsync(query, options.Value, cancellationToken))
    .CollectionQuery()
    .Prefer()
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/weatherforecast/{id:guid}",
        async (Guid id, ResourceQuery query, CancellationToken cancellationToken) =>
            await data.AsQueryable().ToOneAsync(query, id, cancellationToken))
    .ItemQuery()
    .Prefer()
    .WithName("GetWeatherForecastById")
    .WithOpenApi();

app.Run();