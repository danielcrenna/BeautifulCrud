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
            Path = NormalizePathString(request)
        };

        if (request.Host.Port.HasValue)
            uriBuilder.Port = request.Host.Port.Value;

        return uriBuilder.Uri;
    }

    private static PathString NormalizePathString(HttpRequest request)
    {
        var path = request.Path.HasValue 
            ? new PathString(request.Path.Value.TrimEnd('/')) 
            : request.Path;

        if (string.IsNullOrEmpty(path.Value))
            path = new PathString("/");
        return path;
    }
}