using System.Net.Http.Json;
using Demo.Shared;
using Xunit;

namespace BeautifulCrud.IntegrationTests;

public class DemoControllerProjectionTests
{
    [Fact]
    public async Task SelectIsUsedWhenPresent_GetById()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 1; });
        var client = factory.CreateClient();
        
        var firstPageResponse = await client.GetAsync("/weatherforecast");
        firstPageResponse.EnsureSuccessStatusCode();
        
        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(firstPage);
        Assert.NotNull(firstPage.Value);
        Assert.True(firstPage.Items == 1);

        var first = firstPage.Value.First();
        
        var getByIdResponse = await client.GetAsync($"/weatherforecast/{first.Id}/?$select=Date");
        getByIdResponse.EnsureSuccessStatusCode();

        var getByIdContent = await getByIdResponse.Content.ReadAsStringAsync();
        Assert.True(getByIdContent.ContainsAny(["date"]));
        Assert.False(getByIdContent.ContainsAny(["summary", "id", "temperatureC", "temperatureF"]));

        var getById = await getByIdResponse.Content.ReadFromJsonAsync<One<WeatherForecast>>();
        Assert.NotNull(getById);
        Assert.NotNull(getById.Value);
        Assert.Null(getById.Value.Summary);
        Assert.Equal(Guid.Empty, getById.Value.Id);
        Assert.Equal(0, getById.Value.TemperatureC);
        Assert.Equal(32, getById.Value.TemperatureF);
    }

    [Fact]
    public async Task SelectIsUsedWhenPresent_WithoutCount()
    {
        await SelectIsUsedWhenPresent("/weatherforecast/?$count=false");
    }

    [Fact]
    public async Task SelectIsUsedWhenPresent_WithCount()
    {
        await SelectIsUsedWhenPresent("/weatherforecast/?$count=true");
    }

    private static async Task SelectIsUsedWhenPresent(string url)
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 10; });
        var client = factory.CreateClient();

        var firstPageResponse = await client.GetAsync(url);
        firstPageResponse.EnsureSuccessStatusCode();

        var firstPageContent = await firstPageResponse.Content.ReadAsStringAsync();
        Assert.True(firstPageContent.ContainsAny(["date", "summary", "id", "temperatureC", "temperatureF"]));
        
        var secondPageResponse = await client.GetAsync($"{url}&$select=Date");
        secondPageResponse.EnsureSuccessStatusCode();

        var secondPageContent = await secondPageResponse.Content.ReadAsStringAsync();
        Assert.True(secondPageContent.ContainsAny(["date"]));
        Assert.False(secondPageContent.ContainsAny(["summary", "id", "temperatureC", "temperatureF"]));
        
        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(secondPage);
        Assert.NotNull(secondPage.Value);
        Assert.True(secondPage.Items == 10);

        foreach (var value in secondPage.Value)
        {
            Assert.Null(value.Summary);
            Assert.Equal(Guid.Empty, value.Id);
            Assert.Equal(0, value.TemperatureC);
            Assert.Equal(32, value.TemperatureF);
        }
    }
}