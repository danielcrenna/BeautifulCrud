using Xunit;

namespace BeautifulCrud.IntegrationTests.Extensions;

internal static class HttpResponseExtensions
{
    public static async Task AssertSuccessStatusCodeAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            Assert.Fail(await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
    }
}