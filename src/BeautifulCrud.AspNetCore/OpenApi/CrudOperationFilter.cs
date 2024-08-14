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

		MaybeAppendQueryOperators(operation, descriptor);
	}
	
	private void MaybeAppendQueryOperators(OpenApiOperation operation, ActionDescriptor descriptor)
	{
        var isCollectionQuery = Has<CollectionQueryAttribute>(descriptor);

		#region Projection

        if (isCollectionQuery || HasAny<ItemQueryAttribute, ProjectActionFilter, ProjectEndpointFilter>(descriptor))
		{
            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.SelectOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Only include the specified fields in the response body"),
				Example = GetTopLevelPropertyNames(operation)
			});

            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.IncludeOperator,
				In = ParameterLocation.Query,
				Description = _localizer.GetString("Include the specified fields in the response body, in addition to top-level properties"),
				Example = GetTopLevelPropertyNames(operation)
			});

            TryAdd(operation, new OpenApiParameter
			{
				Name = options.CurrentValue.ExcludeOperator,
                In = ParameterLocation.Query,
				Description = _localizer.GetString("Omit the specified fields in the response body"),
				Example = new OpenApiString("")
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
				Description = _localizer.GetString("Apply a property-level filter to the query"),
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

	private static OpenApiString GetTopLevelPropertyNames(OpenApiOperation operation)
	{
		var responseType = operation.Responses.Values.FirstOrDefault()?.Content?.Values.FirstOrDefault()?.Schema;
		if (responseType == null)
			return new OpenApiString("");

		var fieldNames = responseType.Properties.Keys.ToList();
		var example = string.Join(", ", fieldNames);
		return new OpenApiString(example);
	}
}