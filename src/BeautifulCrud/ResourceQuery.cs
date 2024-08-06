using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

#pragma warning disable IDE0060

namespace BeautifulCrud;

public sealed class ResourceQuery
{
    public StringValues? Filter { get; set; }
    public Paging? Paging { get; set; }
    public DateTimeOffset? AsOfDateTime { get; set; }
    public Uri? ServerUri { get; set; }
    public string? NextLink { get; set; }
    public string? DeltaLink { get; set; }

    public List<(PropertyInfo?, string, SortDirection)> Sorting { get; set; } = [];
    public List<ProjectionPath> Projection { get; set; } = [];
    public List<(string, string)> Search { get; set; } = [];
    
    public bool CountTotalRows { get; set; }
    public bool PreferMinimal { get; set; }
    public bool IsDeltaQuery { get; set; }

    // ReSharper disable once UnusedMember.Global
    public static ValueTask<ResourceQuery> BindAsync(HttpContext httpContext, ParameterInfo parameter) => ValueTask.FromResult(httpContext.GetResourceQuery());
}