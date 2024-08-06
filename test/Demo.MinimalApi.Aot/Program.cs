using System.Text.Json.Serialization;
using BeautifulCrud;
using BeautifulCrud.AspNetCore;
using Demo.MinimalApi.Aot;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddBeautifulCrud(o => { o.Features = Features.MinimalApis; });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.UseBeautifulCrud();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var data = Enumerable.Range(1, 50).Select(index =>
    new WeatherForecast
    (
        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        Random.Shared.Next(-20, 55),
        summaries[Random.Shared.Next(summaries.Length)]
    ));

app.MapGet("/weatherforecast",
        async (ResourceQuery query, IOptionsSnapshot<CrudOptions> options, CancellationToken cancellationToken) =>
            await data.AsQueryable().ToManyAsync(query, options.Value, cancellationToken))
    .CollectionQuery()
    .Prefer()
    .WithName("GetWeatherForecast");

app.Run();

[JsonSerializable(typeof(Many<WeatherForecast>))]
[JsonSerializable(typeof(CountMany<WeatherForecast>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;