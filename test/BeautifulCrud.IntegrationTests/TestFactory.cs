using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BeautifulCrud.IntegrationTests;

internal static class TestFactory
{
    public static WebApplicationFactory<T> WithOptions<T>(Action<CrudOptions> configureAction) where T : class
    {
        var factory = new WebApplicationFactory<T>()
            .WithWebHostBuilder(b =>
            {
                b.ConfigureServices(s =>
                {
                    s.Configure(configureAction);
                });
            });

        return factory; }
}