using BeautifulCrud.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BeautifulCrud;

public static class QueryableExtensions
{
    private static readonly IContinuationTokenGenerator ContinuationTokenGenerator =
        new PortableContinuationTokenGenerator(
            () => DateTimeOffset.Now, 
            new Base64QueryHashEncoder(),
            new BinaryResourceQuerySerializer());

    public static IQueryable<T> ApplyQuery<T>(this IQueryable<T> queryable, ResourceQuery query, CrudOptions options, bool fetchOneMore = true) where T : class
    {
        queryable = queryable
            .TryFilter(query);

        queryable = queryable
            .TrySorting(query)
            .TryPaging(query, options, fetchOneMore)
            .TrySelect(query);

        return queryable;
    }

    public static async Task<Many<T>> ToManyAsync<T>(this IQueryable<T> queryable, ResourceQuery query, CrudOptions options, CancellationToken cancellationToken) where T : class
    {
	    var (value, count)=  await queryable.GetAsync(query, options, cancellationToken)
		    .ConfigureAwait(false);

        return count.HasValue
            ? new CountMany<T>(value, count.Value, query.NextLink, query.DeltaLink)
            : new Many<T>(value, query.NextLink, query.DeltaLink);
    }
    
    public static async Task<One<T>> ToOneAsync<T, TKey>(this IQueryable<T> queryable, ResourceQuery query, TKey id, CancellationToken cancellationToken) 
        where T : class, IKeyed<TKey>, new()
    {
        var (value, found)=  await queryable.GetByIdAsync(query, id, cancellationToken)
            .ConfigureAwait(false);

        return new One<T>(value: value);
    }
    
    public static async Task<(T? value, bool found)> GetByIdAsync<T, TKey>(this IQueryable<T> queryable, ResourceQuery query, TKey id, CancellationToken cancellationToken)
        where T : class, IKeyed<TKey>, new()
    {
        queryable = queryable.Where(x => x.Id != null && x.Id.Equals(id));
        queryable = query.PreferMinimal ? queryable.Select(x => new T { Id = x.Id }) : queryable.TrySelect(query);

        var model = queryable.IsEfCoreQueryable()
            ? await queryable.SingleOrDefaultAsync(cancellationToken)
            : queryable.SingleOrDefault();

        return model == null ? (null, false) : query.PreferMinimal ? (null, true) : (model, true);
    }
	
    public static async Task<(List<T>? value, int? count)> GetAsync<T>(this IQueryable<T> queryable, ResourceQuery query, CrudOptions options, CancellationToken cancellationToken) where T : class
    {
	    queryable = queryable.TryFilter(query);

        if (query.PreferMinimal)
        {
            var any = queryable.IsEfCoreQueryable() ? await queryable.AnyAsync(cancellationToken) : queryable.Any();
            if (any)
                return ([], query.CountTotalRows ? 0 : null);
            int? count = query.CountTotalRows ? 0 : null;
            return (null, count);
        }

	    var data = await QueryAsync();

        if (data.Count > query.Paging?.PageSize.GetValueOrDefault(options.DefaultPageSize))
        {
            data.RemoveAt(data.Count - 1);

            NextLink(typeof(T), query, ContinuationTokenGenerator);

            if(query.IsDeltaQuery)
                DeltaLink(typeof(T), query, ContinuationTokenGenerator);
        }

        {
            int? count = null;

            if (query.CountTotalRows || query.IsDeltaQuery)
            {
                count = await CountAsync(queryable);

                if(count <= data.Count && query.IsDeltaQuery)
                    DeltaLink(typeof(T), query, ContinuationTokenGenerator);
            }

            return (data, count);
        }
        
	    async Task<List<T>> QueryAsync()
        {
            var getPage = queryable
			    .TrySorting(query)
			    .TryPaging(query, options, true)
			    .TrySelect(query);

            return getPage.IsEfCoreQueryable() ? await getPage.ToListAsync(cancellationToken) : [.. getPage];
        }

	    async Task<int> CountAsync(IQueryable<T> source) => source.IsEfCoreQueryable() ? await source.CountAsync(cancellationToken) : source.Count();
    }

    private static void NextLink(Type type, ResourceQuery query, IContinuationTokenGenerator continuationTokenGenerator)
    {
        // See: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#981-server-driven-paging
        var baseUrl = query.ServerUri;
        var opaqueUrl = continuationTokenGenerator.Build(type, query);
        var nextLink = $"{baseUrl}/nextLink/{opaqueUrl}";
        query.NextLink = nextLink;
    }

    private static void DeltaLink(Type type, ResourceQuery query, IContinuationTokenGenerator continuationTokenGenerator)
    {
        // See: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#981-server-driven-paging
        var baseUrl = query.ServerUri;
        var opaqueUrl = continuationTokenGenerator.Build(type, query);
        var deltaLink = $"{baseUrl}/deltaLink/{opaqueUrl}";
        query.DeltaLink = deltaLink;
    }

    private static IQueryable<T> TryFilter<T>(this IQueryable<T> queryable, ResourceQuery query) where T : class => string.IsNullOrWhiteSpace(query.Filter) ? queryable : queryable.BuildWhere(query.Filter.Value);

    private static IQueryable<T> TrySorting<T>(this IQueryable<T> queryable, ResourceQuery query) where T : class
    {
        if (query.Sorting is not { Count: > 0 })
            return queryable;

        for (var i = 0; i < query.Sorting.Count; i++)
        {
            var (_, path, direction) = query.Sorting[i];
            var isDescending = direction == SortDirection.Descending;
            queryable = queryable.BuildSort(path, isDescending, i == 0);
        }

        return queryable;
    }

    private static IQueryable<T> TryPaging<T>(this IQueryable<T> queryable, ResourceQuery query, CrudOptions options, bool fetchOneMore) where T : class
    {
        return queryable.TryPaging(query.Paging, options, fetchOneMore);
    }

    private static IQueryable<T> TryPaging<T>(this IQueryable<T> queryable, Paging? paging, CrudOptions options, bool fetchOneMore) where T : class
    {
        if (paging == null)
            return queryable;

        var skip = paging.PageOffset.GetValueOrDefault(0);
        var take = paging.PageSize.GetValueOrDefault(options.DefaultPageSize);

        if (fetchOneMore)
            take++;

        queryable = queryable.Skip(skip);
        queryable = queryable.Take(take);

        return queryable;
    }

    private static IQueryable<T> TrySelect<T>(this IQueryable<T> queryable, ResourceQuery query) where T : class
    {
        if (query.Projection is not { Count: > 0 })
            return queryable;

        var projectionArray = query.Projection.ToArray();
        var selectorLambda = SelectParser.BuildSelect<T>(projectionArray);
        queryable = queryable.Select(selectorLambda);

        return queryable;
    }
}