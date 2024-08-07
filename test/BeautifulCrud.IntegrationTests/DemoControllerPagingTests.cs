using System.Net.Http.Json;
using Demo.Controllers;
using Xunit;

namespace BeautifulCrud.IntegrationTests;

public class DemoControllerPagingTests
{
    [Fact]
    public async Task MaxPageSizeIsUsedWhenLowerThanDefaultPageSize()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 100; });
        var client = factory.CreateClient();
        
        var response = await client.GetAsync("/weatherforecast/?$maxpagesize=50");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(content);
        Assert.True(content.Items == 50);
    }

    [Fact]
    public async Task MaxPageSizeIsNotUsedWhenLargerThanDefaultPageSize()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 10; });
        var client = factory.CreateClient();
        
        var response = await client.GetAsync("/weatherforecast/?$maxpagesize=20");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(content);
        Assert.True(content.Items == 10);
    }

    [Fact]
    public async Task SkipIsUsedWhenPresent()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 10; });
        var client = factory.CreateClient();

        var firstPageResponse = await client.GetAsync("/weatherforecast");
        firstPageResponse.EnsureSuccessStatusCode();

        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(firstPage);
        Assert.NotNull(firstPage.Value);

        Assert.True(firstPage.Items == 10, "First page should have ten items");

        var secondPageResponse = await client.GetAsync("/weatherforecast/?$skip=5");
        secondPageResponse.EnsureSuccessStatusCode();

        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(secondPage);
        Assert.NotNull(secondPage.Value);

        Assert.True(secondPage.Items == 10, "Second page should have ten items");

        var delta = secondPage.Value.Except(firstPage.Value);
        Assert.True(delta.Count() == 5);
    }

    [Fact]
    public async Task TopIsUsedWhenPresentAndLessThanDefaultPageSize()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 10; });
        var client = factory.CreateClient();

        var firstPageResponse = await client.GetAsync("/weatherforecast");
        firstPageResponse.EnsureSuccessStatusCode();

        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(firstPage);
        Assert.NotNull(firstPage.Value);

        Assert.True(firstPage.Items == 10, "First page should have ten items");

        var secondPageResponse = await client.GetAsync("/weatherforecast/?$top=5");
        secondPageResponse.EnsureSuccessStatusCode();

        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(secondPage);
        Assert.NotNull(secondPage.Value);

        Assert.True(secondPage.Items == 5, "Second page should have five items");

        var delta = firstPage.Value.Except(secondPage.Value);
        Assert.True(delta.Count() == 5);
    }

    [Fact]
    public async Task TopIsIgnoredWhenPresentAndGreaterThanDefaultPageSize()
    {
        var factory = TestFactory.WithOptions<Program>(o => { o.DefaultPageSize = 10; });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/weatherforecast/?$top=15");
        response.EnsureSuccessStatusCode();

        var page = await response.Content.ReadFromJsonAsync<Many<WeatherForecast>>();
        Assert.NotNull(page);
        Assert.NotNull(page.Value);

        Assert.True(page.Items == 10, "Page should have ten items, because $top is larger than default page size");
    }
}

