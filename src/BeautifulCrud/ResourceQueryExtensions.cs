using System.Collections;
using System.Dynamic;
using System.Reflection;
using BeautifulCrud.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace BeautifulCrud;

public static class ResourceQueryExtensions
{
	public static ResourceQuery GetResourceQuery(this HttpContext http)
	{
		if (http.Items.TryGetValue(nameof(ResourceQuery), out var resourceQueryObject) &&
			resourceQueryObject is ResourceQuery query)
			return query;

		query = new ResourceQuery();
		http.Items.TryAdd(nameof(ResourceQuery), query);
		return query;
	}

    public static void Parse<T>(this ResourceQuery query, StringValues value, CrudOptions options) => query.Parse(typeof(T), value, options);

    public static void Parse(this ResourceQuery query, Type type, StringValues value, CrudOptions options)
    {
        var queryString = value.AsQueryCollection();

        Parse(query, type, queryString, options);
    }

    public static void Parse(this ResourceQuery query, Type type, IQueryCollection queryString, CrudOptions options)
    {
        query.ApplyProjection(type, queryString, options);
        query.ApplyFilter(queryString, options);
        query.ApplySorting(type, queryString, options);
        query.ApplyPaging(queryString, options);
        query.ApplyCount(queryString, options);
    }

    #region Filter

    public static void ApplyFilter(this ResourceQuery query, IQueryCollection value, CrudOptions options)
    {
        if (!value.TryGetValue(options.FilterOperator, out var clauses) || clauses.Count == 0)
            return;

        query.Filter = clauses;
    }

    #endregion

	#region Projection

	private static readonly char[] Comma = [','];
    
    public static void ApplyProjection(this ResourceQuery query, Type type, IQueryCollection queryString, CrudOptions options)
    { 
        queryString.TryGetValue(options.IncludeOperator, out var include);
        queryString.TryGetValue(options.SelectOperator, out var select);
        queryString.TryGetValue(options.ExcludeOperator, out var exclude);
		
        query.Project(type, include, select, exclude);
    }

    public static void Project<T>(this ResourceQuery query, StringValues value, CrudOptions options) => ApplyProjection(query, typeof(T), value.AsQueryCollection(), options);

    private static void Project(this ResourceQuery query, Type type, StringValues include, StringValues select, StringValues exclude)
    {
        var topLevelFields = type.GetProperties()
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        if (select.Count > 0)
        {
            ProjectIncluded(type, select, query.Projection);
        }
        else if (include.Count > 0)
        {
            var inclusions = topLevelFields.Values
                .Select(property => new ProjectionPath
                {
                    Name = property.Name,
                    Type = property.PropertyType
                })
                .ToList();

            query.Projection.AddRange(inclusions);
            ProjectIncluded(type, include, query.Projection);
        }
        else if (exclude.Count > 0)
        {
            ProjectExcluded(topLevelFields, type, exclude, query.Projection);
        }
    }

    private static void ProjectExcluded(IReadOnlyDictionary<string, PropertyInfo> topLevelProperties, Type type, StringValues clauses, List<ProjectionPath> paths)
    {
        List<ProjectionPath> exclusions = [];

        foreach (var clause in clauses.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var fields = clause!.Split(Comma, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var field in fields)
            {
                var path = FindPropertyNavigation(type, field);
                if (path != null && !IsExcluded(exclusions, path))
                    exclusions.Add(path);
            }
        }

        if (exclusions.Count == 0)
        {
            foreach (var property in topLevelProperties.Values)
            {
                paths.Add(new ProjectionPath
                {
                    Name = property.Name,
                    Type = property.PropertyType
                });
            }
            return;
        }

        foreach (var property in topLevelProperties)
        {
            if (!exclusions.IsExcluded(new ProjectionPath { Name = property.Key, Type = property.Value.PropertyType }))
            {
                paths.Add(new ProjectionPath
                {
                    Name = property.Key,
                    Type = property.Value.PropertyType
                });
            }
        }
    }

    private static bool IsExcluded(this IEnumerable<ProjectionPath> exclusions, ProjectionPath path)
    {
        return exclusions.Any(x => x.Type == path.Type && x.Name == path.Name);
    }

	private static void ProjectIncluded(Type type, StringValues clauses, ICollection<ProjectionPath> paths)
	{
        foreach (var clause in clauses.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var fields = clause!.Split(Comma, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var field in fields)
            {
                var path = FindPropertyNavigation(type, field);
                if (path != null && !paths.Any(x => x.Type == path.Type && x.Name == path.Name))
                {
                    paths.Add(path);
                }
            }
        }
	}

    private static ProjectionPath? FindPropertyNavigation(Type type, string path)
    {
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        ProjectionPath? root = null;
        ProjectionPath? current = null;

        foreach (var part in parts)
        {
            var property = type.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return null;
            }

            var next = new ProjectionPath
            {
                Name = property.Name,
                Type = property.PropertyType
            };

            if (current == null)
                root = next;
            else
                current.Next = next;

            current = next;
            type = property.PropertyType;
        }

        return root;
    }

	public static object Project(this ResourceQuery query, object instance)
	{
		var type = instance.GetType();
		
        switch (type.IsGenericType)
		{
			case true when type.GetGenericTypeDefinition() == typeof(Many<>):
            {
                return ProjectMany(query, instance, type);
            }
			case true when type.GetGenericTypeDefinition() == typeof(CountMany<>):
            {
                return ProjectCountMany(query, instance, type);
            }
            case true when type.GetGenericTypeDefinition() == typeof(One<>):
            {
                return ProjectOne(query, instance, type);
            }
			default:
			{
				if (instance is not IEnumerable<object> enumerable)
					return Project(instance, query.Projection);

				var items = new List<object>();
				foreach (var item in enumerable)
					items.Add(Project(item, query.Projection));

				return items;
			}
		}
	}

    private static One<object> ProjectOne(ResourceQuery query, object instance, Type type)
    {
        var valueProperty = type.GetProperty(nameof(One<object>.Value));
        var errorProperty = type.GetProperty(nameof(One<object>.Error));
                
        var value = valueProperty?.GetValue(instance, null);
        var error = errorProperty?.GetValue(instance, null) as bool?;

        if (value != null)
            value = query.Project(value);

        return new One<object>(value, error.GetValueOrDefault());
    }

    private static CountMany<object> ProjectCountMany(ResourceQuery query, object instance, Type type)
    {
        var valueProperty = type.GetProperty(nameof(CountMany<object>.Value));
        var maxItemsProperty = type.GetProperty(nameof(CountMany<object>.MaxItems));
        var enumerable = valueProperty?.GetValue(instance, null) as IEnumerable;

        var items = new List<object>();
        foreach (var item in enumerable ?? Enumerable.Empty<object>())
            items.Add(query.Project(item));

        var maxItems = maxItemsProperty?.GetValue(instance) as int?;

        return new CountMany<object>(items, maxItems.GetValueOrDefault(), query.NextLink, query.DeltaLink);
    }

    private static Many<object> ProjectMany(ResourceQuery query, object instance, Type type)
    {
        var valueProperty = type.GetProperty(nameof(Many<object>.Value));
        var enumerable = valueProperty?.GetValue(instance, null) as IEnumerable;

        var items = new List<object>();
        foreach (var item in enumerable ?? Enumerable.Empty<object>())
            items.Add(query.Project(item));

        return new Many<object>(items, query.NextLink, query.DeltaLink);
    }

    private static object Project(object instance, List<ProjectionPath> paths)
	{
		var type = instance.GetType();

		IDictionary<string, object?> expando = new ExpandoObject();

		foreach (var path in paths)
		{
			if (path.Name == null)
				continue;

			var property = type.GetProperty(path.Name, BindingFlags.Public | BindingFlags.Instance);
			if (property == null)
				continue;

			var value = property.GetValue(instance);

			if (path.Next != null && value != null)
				value = Project(value, [path.Next]);

			expando[property.Name] = value;
		}

		return expando;
	}

	#endregion

	#region Sorting

	private static readonly char[] Space = [' '];

	public static readonly List<(string, SortDirection)> DefaultSort = [new ValueTuple<string, SortDirection>("Id", SortDirection.Ascending)];

    public static void ApplySorting(this ResourceQuery query, Type type, IQueryCollection value, CrudOptions options)
    {
        query.Sorting.Clear();

        if (value.TryGetValue(options.OrderByOperator, out var clauses))
            query.Sort(type, clauses);

        query.ApplyDefaultSort(type);
    }

	public static void Sort<T>(this ResourceQuery query, StringValues clauses) => query.Sort(typeof(T), clauses);

	public static void Sort(this ResourceQuery query, Type type, StringValues clauses)
	{
		if (clauses.Count <= 0)
			return;

		foreach (var value in clauses.Where(x => !string.IsNullOrWhiteSpace(x)))
		{
			var tokens = value!.Split(Space, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			if (tokens.Length == 0)
				continue;

			var clause = tokens[0];
			var name = clause[(clause.IndexOf('=', StringComparison.Ordinal) + 1)..];
			var sort = tokens.Length > 1 ? tokens[1].ToUpperInvariant() : "ASC";

			var propertyPath = GetNestedPropertyPath(type, name);
			if (propertyPath != null)
				switch (sort)
				{
					case "DESC":
						query.Sorting.Add((propertyPath.Value.Item1, propertyPath.Value.Item2,
							SortDirection.Descending));
						break;
					case "ASC":
						query.Sorting.Add((propertyPath.Value.Item1, propertyPath.Value.Item2,
							SortDirection.Ascending));
						break;
					default:
						query.Sorting.Add((propertyPath.Value.Item1, propertyPath.Value.Item2,
							SortDirection.Ascending));
						break;
				}
		}
	}

	public static void ApplyDefaultSort(this ResourceQuery query, Type type)
	{
		// Default sort must always run after any user-specified sorts:
		// -------------------------------------------------------------------
		// See: https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#983-additional-considerations
		// "Stable order prerequisite: Both forms of paging depend on the collection of items having a stable order. The server
		// MUST supplement any specified order criteria with additional sorts (typically by key) to ensure that items are always
		// ordered consistently."
		//
		foreach (var (field, direction) in DefaultSort)
		{
			var propertyPath = GetNestedPropertyPath(type, field);
			if (propertyPath != null && !query.Sorting.Any(x => x.Item2.Equals(propertyPath.Value.Item2, StringComparison.OrdinalIgnoreCase)))
				query.Sorting.Add((propertyPath.Value.Item1, propertyPath.Value.Item2, direction));
		}
	}

	private static (PropertyInfo?, string)? GetNestedPropertyPath(Type type, string propertyName)
	{
		PropertyInfo? property = null;
		var currentType = type;
		var propertyPath = new List<string>();

		foreach (var part in propertyName.Split('.'))
		{
			property = currentType.GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (property == null)
				return null;

			propertyPath.Add(property.Name);
			currentType = property.PropertyType;
		}

		return (property, string.Join(".", propertyPath));
	}

	#endregion

	#region Paging

    public static void ApplyPaging(this ResourceQuery query, IQueryCollection value, CrudOptions options) => query.Paging(value, options);

    public static void Paging(this ResourceQuery query, StringValues value, CrudOptions options) => query.Paging(value.AsQueryCollection(), options);

    public static void Paging(this ResourceQuery query, IQueryCollection queryString, CrudOptions options)
    {
        queryString.TryGetValue(options.MaxPageSizeOperator, out var maxPageSize);
        queryString.TryGetValue(options.SkipOperator, out var skip);
        queryString.TryGetValue(options.TopOperator, out var top);
		
        query.Paging(maxPageSize, skip, top, options);
    }

    private static void Paging(this ResourceQuery query, StringValues maxPageSizeClause, StringValues skipClauses, StringValues topClauses, CrudOptions options)
	{
		query.Paging ??= new Paging();

		{
			if (maxPageSizeClause.Count > 0)
			{
				// Clients MAY request server-driven paging with a specific page size by specifying a $maxpagesize preference.
                // The server SHOULD honor this preference if the specified page size is smaller than the server's default page size.
				
				var maxPageSize = options.DefaultPageSize;

				if (maxPageSizeClause.Count == 1 && int.TryParse(maxPageSizeClause[0], out var pageSize) && pageSize < maxPageSize)
					maxPageSize = pageSize;

				query.Paging.MaxPageSize = maxPageSize;
			}
		}

		{
			if (skipClauses.Count > 0)
				if (skipClauses.Count == 1 && int.TryParse(skipClauses[0], out var skip))
					query.Paging.PageOffset = skip;
		}

		{
			if (topClauses.Count > 0)
			{
				var pageSize = options.DefaultPageSize;

				if (topClauses.Count == 1 && int.TryParse(topClauses[0], out var top))
					pageSize = top;

				//
				// "Note that client-driven paging does not preclude server-driven paging.
				// If the page size requested by the client is larger than the default page size
				// supported by the server, the expected response would be the number of results specified by the client,
				// paginated as specified by the server paging settings."
				//
				if (pageSize > options.DefaultPageSize)
					pageSize = options.DefaultPageSize;

				query.Paging.PageSize = pageSize;
			}
		}

		// If no $skip is provided, assume the query is for the first page
		query.Paging.PageOffset ??= 0;

		//
		// "Clients MAY request server-driven paging with a specific page size by specifying a $maxpagesize preference.
		//  The server SHOULD honor this preference if the specified page size is smaller than the server's default page size."
		//
		// Interpretation:
		// - If the client specifies $top, use $top as the page size.
		// - If the client omits $top but specifies $maxpagesize, use the smallest of $maxpagesize and the server's default page size.
		//
        query.Paging.PageSize ??= query.Paging.MaxPageSize < options.DefaultPageSize
            ? query.Paging.MaxPageSize.Value
            : options.DefaultPageSize;
	}

	#endregion

    #region Count

    public static void ApplyCount(this ResourceQuery query, IQueryCollection value, CrudOptions options)
    {
        if (!value.TryGetValue(options.CountOperator, out var clauses))
            return;

        if (clauses.Count <= 0)
            return;

        foreach (var clause in clauses.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (clause != null && (clause.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                   int.TryParse(clause, out var countAsNumber) && countAsNumber == 1 ||
                                   clause.Equals("yes", StringComparison.OrdinalIgnoreCase)))

                query.CountTotalRows = true;
        }
    }

    #endregion
}