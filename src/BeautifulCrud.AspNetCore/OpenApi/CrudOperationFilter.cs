using BeautifulCrud.AspNetCore.ActionFilters;
using BeautifulCrud.AspNetCore.Attributes;
using BeautifulCrud.AspNetCore.EndpointFilters;
using BeautifulCrud.AspNetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BeautifulCrud.AspNetCore.OpenApi;

public sealed class CrudOperationFilter(IServiceProvider serviceProvider, IOptionsMonitor<CrudOptions> options) : IOperationFilter
{
    private readonly IStringLocalizer<CrudOperationFilter> _localizer = serviceProvider.GetService<IStringLocalizer<CrudOperationFilter>>() ?? 
                                                                   new NoStringLocalizer<CrudOperationFilter>();
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var descriptor = context.ApiDescription.ActionDescriptor;

		MaybeAppendQueryOperators(context, operation, descriptor);
	}
	
	private void MaybeAppendQueryOperators(OperationFilterContext context, OpenApiOperation operation, ActionDescriptor descriptor)
	{
        var isCollectionQuery = Has<CollectionQueryAttribute>(descriptor);

		#region Projection

        if (isCollectionQuery || HasAny<ItemQueryAttribute, ProjectActionFilter, ProjectEndpointFilter>(descriptor))
		{
            TryAdd(operation, new OpenApiParameter
            {
                Name = options.CurrentValue.SelectOperator,
                In = ParameterLocation.Query,
                Description = _localizer.GetString("Only include the specified fields in the response body. If this is specified, include and exclude are ignored."),
                Examples = GetProjectionExamples(options.CurrentValue.SelectOperator, context, operation)
            });

            TryAdd(operation, new OpenApiParameter
            {
                Name = options.CurrentValue.IncludeOperator,
                In = ParameterLocation.Query,
                Description = _localizer.GetString("Include the specified fields in the response body, in addition to top-level fields. If this is specified, exclude is ignored."),
                Examples = GetProjectionExamples(options.CurrentValue.IncludeOperator, context, operation)
            });

            TryAdd(operation, new OpenApiParameter
            {
                Name = options.CurrentValue.ExcludeOperator,
                In = ParameterLocation.Query,
                Description = _localizer.GetString("Omits the specified fields in the response body"),
                Examples = GetProjectionExamples(options.CurrentValue.ExcludeOperator, context, operation)
            });
		}

		#endregion

		#region Filter

		if (isCollectionQuery || HasAny<FilterAttribute, FilterActionFilter, FilterEndpointFilter>(descriptor))
		{
            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.FilterOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Apply a field-level filter to the query"),
				Example = new OpenApiString("")
			});
		}

		#endregion
		
		#region Sorting

		if (isCollectionQuery || HasAny<SortAttribute, SortingActionFilter, SortingEndpointFilter>(descriptor))
		{
            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.OrderByOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Apply a field-level sort to the query"),
				Example = new OpenApiString("id asc")
			});
		}

		#endregion

		#region Paging

		if (isCollectionQuery || HasAny<PagingAttribute, PagingActionFilter, PagingEndpointFilter>(descriptor))
		{
            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.MaxPageSizeOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Request server-driven paging with a specific page size"),
				Example = new OpenApiString("")
			});

            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.SkipOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Apply a client-directed offset into a collection."),
				Example = new OpenApiString("")
			});

            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.TopOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Apply a client-directed request to specify the number of results to return."),
				Example = new OpenApiString("")
			});
		}

		#endregion

		#region Count
		
		if (isCollectionQuery || HasAny<CountAttribute, CountActionFilter, CountEndpointFilter>(descriptor))
		{
            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.CountOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Requests the server to include the total count of all items across pages in the response"),
				Example = new OpenApiString("false")
			});
		}

		#endregion

        if (HasAny<PreferAttribute, PreferActionFilter, PreferEndpointFilter>(descriptor))
        {
            TryAdd(operation, new OpenApiParameter
            {
                Name = "Prefer",
                In = ParameterLocation.Header,
                Description = _localizer.GetString("Apply a preference for returning the representation. Valid values are 'minimal' and 'representation'"),
                Example = new OpenApiString("return=representation")
            });
        }
	}

    private static Dictionary<string, OpenApiExample> GetProjectionExamples(string @operator, OperationFilterContext context, OpenApiOperation operation)
    {
        var fieldNames = ResolveFieldNames(operation, context.SchemaRepository)
            .ToList();

        var examples = new Dictionary<string, OpenApiExample>
        {
            { @operator, new OpenApiExample { Value = new OpenApiString("")} }
        };

        foreach (var fieldName in fieldNames)
            examples.Add(fieldName.Value, new OpenApiExample { Value = fieldName });

        return examples;
    }

    private static void TryAdd(OpenApiOperation operation, OpenApiParameter parameter)
    {
		if(operation.Parameters.Any(x => x.Name == parameter.Name))
            return;
        operation.Parameters.Add(parameter);
    }

    private static bool Has<TAttribute>(ActionDescriptor descriptor)
    {
        return descriptor.EndpointMetadata.OfType<TAttribute>().Any();
    }
	
	private static bool HasAny<TAttribute, TServiceFilter, TEndpointFilter>(ActionDescriptor descriptor)
	{
		return descriptor.EndpointMetadata.OfType<TAttribute>().Any() || 
		       descriptor.EndpointMetadata.OfType<ServiceFilterAttribute>().Any(x => x.ServiceType == typeof(TServiceFilter)) ||
		       descriptor.EndpointMetadata.OfType<TEndpointFilter>().Any()
		       ;
	}

	private static IEnumerable<OpenApiString> ResolveFieldNames(OpenApiOperation operation, SchemaRepository schemas)
	{
        var responses = operation.Responses.Values;
        var mediaTypes = responses.FirstOrDefault()?.Content;

        var schema = mediaTypes?.Values.FirstOrDefault()?.Schema;
        if (schema == null)
            return [];

        var fieldNames = ResolveFieldNames(schema, schemas);
        var examples = fieldNames.Select(x => new OpenApiString(x));
        return examples;
    }

    public static List<string> ResolveFieldNames(OpenApiSchema schema, SchemaRepository schemas)
    {
        while (true)
        {
            var fieldNames = new List<string>();

            if (schema.Items != null)
            {
                schema = schema.Items;
                continue;
            }

            if (schema.Reference != null)
            {
                if (!schemas.Schemas.TryGetValue(schema.Reference.Id, out var reference))
                    return fieldNames;

                if (reference.Properties.TryGetValue("value", out var value) && value != null)
                {
                    schema = value;
                    continue;
                }

                foreach (var field in reference.Properties)
                    fieldNames.Add(field.Key);
            }
            else
            {
                foreach (var field in schema.Properties)
                    fieldNames.Add(field.Key);
            }

            return fieldNames;
        }
    }
}