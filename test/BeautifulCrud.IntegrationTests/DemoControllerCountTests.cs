using System.Net.Http.Json;
using Demo.Shared;
using Xunit;

namespace BeautifulCrud.IntegrationTests;

public class DemoControllerCountTests
{
    [Fact]
    public async Task CountReturnsCountMany()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 25; });
        var client = factory.CreateClient();
        
        var response = await client.GetAsync("/weatherforecast/?$count=true");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<CountMany<WeatherForecast>>();
        Assert.NotNull(content);
        Assert.NotNull(content.Value);
        Assert.Equal(25, content.Items);
        Assert.Equal(50, content.MaxItems);
    }
}