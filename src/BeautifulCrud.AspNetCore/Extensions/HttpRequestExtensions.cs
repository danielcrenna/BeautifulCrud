using Microsoft.AspNetCore.Http;

namespace BeautifulCrud.AspNetCore.Extensions;

internal static class HttpRequestExtensions
{
    public static Uri GetServerUri(this HttpRequest request)
    {
        var uriBuilder = new UriBuilder
        {
            Scheme = request.Scheme,
            Host = request.Host.Host,
            Path = request.Path
        };

        if (request.Host.Port.HasValue)
            uriBuilder.Port = request.Host.Port.Value;

        return uriBuilder.Uri;
    }
}